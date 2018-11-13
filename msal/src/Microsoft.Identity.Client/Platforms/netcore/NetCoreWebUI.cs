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
            // todo implement
            SystemBrowser systemBrowser = new SystemBrowser();
            string result = await systemBrowser.InvokeAsync(authorizationUri.OriginalString).ConfigureAwait(true);

            return null;
        }
    }

    internal class SystemBrowser
    {
        public int Port { get; }
        private readonly string _path;

        public SystemBrowser(int? port = null, string path = null)
        {
            _path = path;

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

        public async Task<string> InvokeAsync(string uri)
        {
            using (var listener = new KestrelBasedListener(Port, _path))
            {
                OpenBrowser(uri);

                return await listener.WaitForCallbackAsync().ConfigureAwait(true);

                //try
                //{
                 
                //}
                //catch (TaskCanceledException ex)
                //{
                //    return new BrowserResult { ResultType = BrowserResultType.Timeout, Error = ex.Message };
                //}
                //catch (Exception ex)
                //{
                //    return new BrowserResult { ResultType = BrowserResultType.UnknownError, Error = ex.Message };
                //}
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
        const int DefaultTimeout = 60 * 5; // 5 mins (in seconds)

        IWebHost _host;
        TaskCompletionSource<string> _source = new TaskCompletionSource<string>();
        string _url;

        public string Url => _url;

        public KestrelBasedListener(int port, string path = null)
        {
            path = path ?? String.Empty;
            if (path.StartsWith("/")) path = path.Substring(1);

            _url = $"http://127.0.0.1:{port}/{path}";

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
                    SetResult(ctx.Request.QueryString.Value, ctx);
                }
                else if (ctx.Request.Method == "POST")
                {
                    if (!ctx.Request.ContentType.Equals("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase))
                    {
                        ctx.Response.StatusCode = 415;
                    }
                    else
                    {
                        using (var sr = new StreamReader(ctx.Request.Body, Encoding.UTF8))
                        {
                            var body = await sr.ReadToEndAsync().ConfigureAwait(true);
                            SetResult(body, ctx);
                        }
                    }
                }
                else
                {
                    ctx.Response.StatusCode = 405;
                }
            });
        }

        private void SetResult(string value, HttpContext ctx)
        {
            try
            {
                ctx.Response.StatusCode = 200;
                ctx.Response.ContentType = "text/html";
                ctx.Response.WriteAsync("<h1>You can now return to the application.</h1>");
                ctx.Response.Body.Flush();

                _source.TrySetResult(value);
            }
            catch
            {
                ctx.Response.StatusCode = 400;
                ctx.Response.ContentType = "text/html";
                ctx.Response.WriteAsync("<h1>Invalid request.</h1>");
                ctx.Response.Body.Flush();
            }
        }

        public Task<string> WaitForCallbackAsync(int timeoutInSeconds = DefaultTimeout)
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
