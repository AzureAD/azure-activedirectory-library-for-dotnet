using Android.App;
using Android.Content;
using Android.Webkit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Identity.Core.Platforms.Android
{
    internal abstract class CoreAuthenticationAgentActivity : Activity
    {
        private const string AboutBlankUri = "about:blank";

        public const string ClientTlsNotSupported = " PKeyAuth/1.0";

        protected WebSettings CreateCoreWebSettings(WebView webView)
        {
            WebSettings webSettings = webView.Settings;
            string userAgent = webSettings.UserAgentString;
            webSettings.UserAgentString =
                    userAgent + ClientTlsNotSupported;
            CoreLoggerBase.Default.Verbose("UserAgent:" + webSettings.UserAgentString);

            webSettings.JavaScriptEnabled = true;

            webSettings.LoadWithOverviewMode = true;
            webSettings.DomStorageEnabled = true;
            webSettings.UseWideViewPort = true;
            webSettings.BuiltInZoomControls = true;
            return webSettings;
        }
    }

    internal abstract class CoreWebViewClient : WebViewClient
    {
    }
}
