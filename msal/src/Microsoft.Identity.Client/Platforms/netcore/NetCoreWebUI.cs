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
using System.Text.RegularExpressions;

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
            if (redirectUri.IsDefaultPort)
            {
                throw new InvalidOperationException("Cannot listen to localhost (no port), please call UpdateRedirectUri to get a free localhost:port address");
            }
            AuthorizationResult result = await _systemBrowser.InvokeAsync(authorizationUri, redirectUri).ConfigureAwait(true);

            return result;
        }

        //TODO: add a validation against non-loopback uri (in PlatformProxy)
        //TODO: make this part of the IWebUI interface 
        //TODO: test with 127.0.0.1 and localhost, or decide if we want to support only "localhost"
        public Uri UpdateRedirectUri(Uri uri)
        {
            if (uri.IsLoopback && uri.IsDefaultPort)
            {
                UriBuilder uriBuilder = new UriBuilder(uri);
                uriBuilder.Port = GetRandomUnusedPort();
                return uriBuilder.Uri;
            }

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

            if (redirectUri.IsDefaultPort)
            {
                throw new ArgumentException("Port required");
            }

            using (var listener = new TcpBasedListener(redirectUri.Port))
            //using (var listener = new KestrelBasedListener(redirectUri.Port))
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

        const int DefaultTimeout = 60 * 5; //TODO: make configurable?

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

    internal class TcpBasedListener : IHttpListener, IDisposable
    {
        TaskCompletionSource<AuthenticationResult> _taskCompletionSource;
        const int DefaultTimeout = 2 * 5; //TODO: configurable?
        int _port;
        private readonly TcpListener _listener;

        public TcpBasedListener(int port)
        {
            if (port < 1 || port == 80)
            {
                throw new ArgumentOutOfRangeException("Expected a valid port number, > 0, not 80");
            }

            _taskCompletionSource = new TaskCompletionSource<AuthenticationResult>();
            _port = port;
            _listener = new TcpListener(IPAddress.Loopback, _port);
        }

        public async Task<AuthorizationResult> WaitForCallbackAsync(int timeoutInSeconds = DefaultTimeout)
        {
            _listener.Start();

            using (TcpClient client = await _listener.AcceptTcpClientAsync().ConfigureAwait(false))
            {
                string httpRequest = await GetTcpResponseAsync(client).ConfigureAwait(false);
                string uri = ExtractUriFromHttpRequest(httpRequest);

                AuthorizationResult authenticationResult = new AuthorizationResult(AuthorizationStatus.Success, uri);

                return authenticationResult;

                //TODO: errors
            }            
        }

#pragma warning disable CS1570 // XML comment has badly formed XML
                              /// <summary>
                              /// Example TCP response:
                              /// 
                              /// {GET /?code=OAQABAAIAAAC5una0EUFgTIF8ElaxtWjTl5wse5YHycjcaO_qJukUUexKz660btJtJSiQKz1h4b5DalmXspKis-bS6Inu8lNs4CpoE4FITrLv00Mr3MEYEQzgrn6JiNoIwDFSl4HBzHG8Kjd4Ho65QGUMVNyTjhWyQDf_12E8Gw9sll_sbOU51FIreZlVuvsqIWBMIJ8mfmExZBSckofV6LbcKJTeEZKaqjC09x3k1dpsCNJAtYTQIus5g1DyhAW8viDpWDpQJlT55_0W4rrNKY3CSD5AhKd3Ng4_ePPd7iC6qObfmMBlCcldX688vR2IghV0GoA0qNalzwqP7lov-yf38uVZ3ir6VlDNpbzCoV-drw0zhlMKgSq6LXT7QQYmuA4RVy_7TE9gjQpW-P0_ZXUHirpgdsblaa3JUq4cXpbMU8YCLQm7I2L0oCkBTupYXKLoM2gHSYPJ5HChhj1x0pWXRzXdqbx_TPTujBLsAo4Skr_XiLQ4QPJZpkscmXezpPa5Z87gDenUBRBI9ppROhOksekMbvPataF0qBaM38QzcnzeOCFyih1OjIKsq3GeryChrEtfY9CL9lBZ6alIIQB4thD__Tc24OUmr04hX34PjMyt1Z9Qvr76Pw0r7A52JvqQLWupx8bqok6AyCwqUGfLCPjwylSLA7NYD7vScAbfkOOszfoCC3ff14Dqm3IAB1tUJfCZoab61c6Mozls74c2Ujr3roHw4NdPuo-re5fbpSw5RVu8MffWYwXrO3GdmgcvIMkli2uperucLldNVIp6Pc3MatMYSBeAikuhtaZiZAhhl3uQxzoMhU-MO9WXuG2oIkqSvKjghxi1NUhfTK4-du7I5h1r0lFh9b3h8kvE1WBhAIxLdSAA&state=b380f309-7d24-4793-b938-e4a512b2c7f6&session_state=a442c3cd-a25e-4b88-8b33-36d194ba11b2 HTTP/1.1
                              /// Host: localhost:9001
                              /// Accept-Language: en-GB,en;q=0.9,en-US;q=0.8,ro;q=0.7,fr;q=0.6
                              /// Connection: keep-alive
                              /// Upgrade-Insecure-Requests: 1
                              /// User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.102 Safari/537.36
                              /// Accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8
                              /// Accept-Encoding: gzip, deflate, br
                              /// </summary>
                              /// <returns>http://localhost:9001/?code=foo&session_state=bar</returns>
        private string ExtractUriFromHttpRequest(string httpRequest)
#pragma warning restore CS1570 // XML comment has badly formed XML
        {
            string regexp = @"GET \/\?(.*) HTTP";
            string getQuery = null;
            Regex r1 = new Regex(regexp);
            Match match = r1.Match(httpRequest);
            if (!match.Success)
            {
                throw new InvalidOperationException("Not a GET query");// TODO: exceptions
            }

            getQuery = match.Groups[1].Value;
            UriBuilder uriBuilder = new UriBuilder();
            uriBuilder.Query = getQuery;
            uriBuilder.Port = _port;
            Uri u = uriBuilder.Uri;

            return uriBuilder.ToString();
        }

        private static async Task<string> GetTcpResponseAsync(TcpClient client)
        {
            NetworkStream networkStream = client.GetStream();

            byte[] readBuffer = new byte[1024];
            StringBuilder stringBuilder = new StringBuilder();
            int numberOfBytesRead = 0;

            // Incoming message may be larger than the buffer size. 
            do
            {
                numberOfBytesRead = await networkStream.ReadAsync(readBuffer, 0, readBuffer.Length).ConfigureAwait(false);

                string s = Encoding.ASCII.GetString(readBuffer, 0, numberOfBytesRead);
                stringBuilder.Append(s);

            }
            while (networkStream.DataAvailable);

            return stringBuilder.ToString();
        }

        public void Dispose()
        {
            _listener?.Stop();
        }
    }
}
