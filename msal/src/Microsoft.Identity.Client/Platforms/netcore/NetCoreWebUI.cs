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
        private RequestContext _requestContext;

        public NetCoreWebUI(RequestContext requestContext)
        {
            _requestContext = requestContext;
        }

        public async Task<AuthorizationResult> AcquireAuthorizationAsync(Uri authorizationUri, Uri redirectUri, RequestContext requestContext)
        {
            SystemBrowser systemBrowser = new SystemBrowser(9001); //TODO: hardcoded port
            AuthorizationResult result = await systemBrowser.InvokeAsync(authorizationUri.OriginalString).ConfigureAwait(true);

            return result;
        }
    }


    //TODO: figure out a way to let the user configure the messages on this
    //TODO: we can let the user chose his own port - maybe as a separate story?
    internal class SystemBrowser
    {
        public int Port { get; }

        public SystemBrowser(int? port = null)
        {
            if (!port.HasValue)
            {
                Port = GetRandomUnusedPort();
            }
            else
            {
                Port = port.Value;
            }
        }

        private int GetRandomUnusedPort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        //public async Task<BrowserResult> InvokeAsync(BrowserOptions options)
        //{
        //    using (var listener = new KestrelBasedListener(Port, _path))
        //    {
        //        OpenBrowser(options.StartUrl);

        //        try
        //        {
        //            var result = await listener.WaitForCallbackAsync().ConfigureAwait(true);
        //            if (String.IsNullOrWhiteSpace(result))
        //            {
        //                return new BrowserResult { ResultType = BrowserResultType.UnknownError, Error = "Empty response." };
        //            }

        //            return new BrowserResult { Response = result, ResultType = BrowserResultType.Success };
        //        }
        //        catch (TaskCanceledException ex)
        //        {
        //            return new BrowserResult { ResultType = BrowserResultType.Timeout, Error = ex.Message };
        //        }
        //        catch (Exception ex)
        //        {
        //            return new BrowserResult { ResultType = BrowserResultType.UnknownError, Error = ex.Message };
        //        }
        //    }
        //}

        public async Task<AuthorizationResult> InvokeAsync(string uri)
        {
            using (var listener = new KestrelBasedListener(Port))
            {
                OpenBrowser(uri);

                try
                {
                    return await listener.WaitForCallbackAsync().ConfigureAwait(true);
                }
                catch (TaskCanceledException ex)
                {
                    // TODO: log ex
                    return await Task.FromResult(new AuthorizationResult(AuthorizationStatus.UserCancel)).ConfigureAwait(false);
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

    //TODO: check that all configureawait(true) can be switched to false
    internal class KestrelBasedListener : IDisposable
    {
        const int DefaultTimeout = 60 * 5; //TODO: configurable?

        IWebHost _host;
        TaskCompletionSource<AuthorizationResult> _source = new TaskCompletionSource<AuthorizationResult>();
        string _url;

        public string Url => _url;

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
                await Task.Delay(500).ConfigureAwait(true);
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
                    await ctx.Response.WriteAsync("<h1>Invalid request. Expecting a GET method.</h1>").ConfigureAwait(false);
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
                ctx.Response.WriteAsync("<h1>Authentication complete. Please close this tab or the browser and return to the application.</h1>");
                ctx.Response.Body.Flush();

            }
            catch (Exception e)
            {
                ctx.Response.StatusCode = 400;
                ctx.Response.ContentType = "text/html";
                ctx.Response.WriteAsync("<h1>Invalid request.</h1>");
                ctx.Response.Body.Flush();

                authorizationResult = new AuthorizationResult(AuthorizationStatus.UnknownError);
            }

            _source.TrySetResult(authorizationResult);

        }

        public Task<AuthorizationResult> WaitForCallbackAsync(int timeoutInSeconds = DefaultTimeout)
        {
            Task.Run(async () =>
            {
                await Task.Delay(timeoutInSeconds * 1000).ConfigureAwait(true);
                _source.TrySetCanceled();
            });

            return _source.Task;
        }
    }

    ////TODO: add cancelation support
    //internal class RawTcpListener : IDisposable
    //{
    //    // RFC7230 recommends supporting a request-line length of at least 8,000 octets
    //    // https://tools.ietf.org/html/rfc7230#section-3.1.1
    //    private const int MaxRequestLineLength = 16 * 1024;
    //    private const int MaxHeadersLength = 64 * 1024;
    //    private const int NetworkReadBufferSize = 1024;


    //    private readonly ICoreLogger _logger;
    //    private readonly TcpListener _tcpListener;
    //    private readonly int _port;

    //    internal RawTcpListern(ICoreLogger logger, Uri uri)
    //    {
    //        _logger = logger;

    //        // TODO: ctor does too much, move to a factory method
    //        if (!uri.IsLoopback)
    //        {
    //            throw new InvalidOperationException("only loopback!");
    //        }

    //        _tcpListener = new TcpListener(IPAddress.Loopback, uri.Port);
    //        _tcpListener.Start();
    //        _port = uri.Port;
    //    }

         

    //    public void Dispose()
    //    {
    //        if (_tcpListener != null)
    //        {
    //            _tcpListener.Stop();
    //        }
    //    }
    //}
}
