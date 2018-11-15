using Microsoft.Identity.Core;
using Microsoft.Identity.Core.UI;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Http.Extensions;

namespace Microsoft.Identity.Client.Internal.UI
{
    internal class NetCoreWebUI : IWebUI
    {
        // TODO: review all errors, make sure to add AuthroizationResult
        private RequestContext _requestContext;
        private SystemBrowser _systemBrowser; 

        public NetCoreWebUI(RequestContext requestContext)
        {
            _requestContext = requestContext;
            _systemBrowser = new SystemBrowser();
        }

        public async Task<AuthorizationResult> AcquireAuthorizationAsync(Uri authorizationUri, Uri redirectUri, RequestContext requestContext)
        {
            //if (redirectUri.IsDefaultPort)
            //{
            //    throw new InvalidOperationException("Cannot listen to localhost (no port), please call UpdateRedirectUri to get a free localhost:port address");
            //}
            AuthorizationResult result = await _systemBrowser.InvokeAsync(authorizationUri, redirectUri).ConfigureAwait(true);

            return result;
        }

        //TODO: add a validation against non-loopback uri (in PlatformProxy)
        //TODO: make this part of the IWebUI interface 
        //TODO: test with 127.0.0.1 and localhost, or decide if we want to support only "localhost"
        public Uri UpdateRedirectUri(Uri uri)
        {
            //if (uri.IsLoopback)
            //{
            //    UriBuilder uriBuilder = new UriBuilder(uri);
            //    uriBuilder.Port = GetRandomUnusedPort();
            //    return uriBuilder.Uri;
            //}

            return uri;
        }


        private int GetRandomUnusedPort() // TODO: test with port being already in use
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

    }


    //TODO: figure out a way to let the user configure the messages on this
    //TODO: we can let the user chose his own port - maybe as a separate story?
    internal class SystemBrowser
    {
        public async Task<AuthorizationResult> InvokeAsync(Uri authorizationUri, Uri redirectUri)
        {
            if (!redirectUri.IsLoopback)
            {
                throw new ArgumentException("Only loopback redirect uri");
            }

            //if (redirectUri.IsDefaultPort)
            //{
            //    throw new ArgumentException("Port required");
            //}

            using (var listener = new KestrelBasedListener(redirectUri.Port))
            {
                OpenBrowser(authorizationUri.OriginalString);

                try
                {
                    return await listener.WaitForCallbackAsync().ConfigureAwait(true);
                }
                catch (TaskCanceledException ex)
                {
                    // TODO: log ex
                    return await Task.FromResult(new AuthorizationResult(AuthorizationStatus.UserCancel)).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    var result = new AuthorizationResult(AuthorizationStatus.UnknownError)
                    {
                        ErrorDescription = ex.Message,
                        Error = "system_browser_waiting_exception"
                    };

                    return await Task.FromResult(result).ConfigureAwait(false);
                }

            }
        }

        public static void OpenBrowser(string url)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw new PlatformNotSupportedException(RuntimeInformation.OSDescription);
                }
            }
        }
    }

    //TODO: add cancelability and see how it propagates up, probably need overloads 
    internal class KestrelBasedListener : IDisposable, IHttpListener  
    {
        private const string CloseWindowSuccessHtml = @"<html>
  <head><title>Authentication Complete</title></head>
  <body>
    Authentication complete. You can return to the application. Feel free to close this browser tab.
  </body>
</html>";

        const int DefaultTimeout = 2 * 5; //TODO: configurable?

        IWebHost _host;
        TaskCompletionSource<AuthorizationResult> _source = new TaskCompletionSource<AuthorizationResult>();
        string _url;

        public KestrelBasedListener(int port)
        {
            _url = $"http://127.0.0.1:{port}";

            _host = new WebHostBuilder()
                .UseKestrel()
                .UseUrls(_url)
                .Configure(Configure)
                .Build();
            _host.Start();
        }

        public void Dispose()
        {
            Task.Run(async () =>
            {
                await Task.Delay(500).ConfigureAwait(false);
                _host.Dispose();
            });
        }

        private void Configure(IApplicationBuilder app)
        {
            app.Run(async ctx =>
            {
                if (ctx.Request.Method == "GET")
                {
                    string fullUri = ctx.Request.GetEncodedUrl();
                    SetResult(fullUri, ctx);
                }
                else
                {
                    ctx.Response.StatusCode = 405;
                    ctx.Response.ContentType = "text/html";
                    await ctx.Response.WriteAsync("Internal authenticaiton error, unsupported method").ConfigureAwait(false);
                    ctx.Response.Body.Flush();

                    _source.TrySetResult(new AuthorizationResult(AuthorizationStatus.ErrorHttp));
                }

            });
        }

        private void SetResult(string uri, HttpContext ctx)
        {
            AuthorizationResult authorizationResult;
            try
            {
                authorizationResult = new AuthorizationResult(AuthorizationStatus.Success, uri);

                ctx.Response.StatusCode = 200;
                ctx.Response.ContentType = "text/html";
                ctx.Response.WriteAsync(CloseWindowSuccessHtml);
                ctx.Response.Body.Flush();

            }
            catch (Exception e)
            {
                ctx.Response.StatusCode = 400;
                ctx.Response.ContentType = "text/html";
                ctx.Response.WriteAsync("<h1>Invalid request.</h1>");
                ctx.Response.Body.Flush();

                authorizationResult = new AuthorizationResult(AuthorizationStatus.UnknownError) { ErrorDescription = e.Message };
            }

            _source.TrySetResult(authorizationResult);

        }



        public Task<AuthorizationResult> WaitForCallbackAsync(int timeoutInSeconds = DefaultTimeout)
        {
            Task.Run(async () =>
            {
                await Task.Delay(timeoutInSeconds * 1000).ConfigureAwait(false);                
                _source.TrySetResult(new AuthorizationResult(AuthorizationStatus.Timeout)
                {
                    Error = "timeout",
                    ErrorDescription = " timeout after " + timeoutInSeconds
                });                
            });

            return _source.Task;
        }
    }

    internal class TcpBasedListener : IHttpListener
    {
        public Task<AuthorizationResult> WaitForCallbackAsync(int timeoutInSeconds = 300)
        {
            throw new NotImplementedException();
        }
    }
}
