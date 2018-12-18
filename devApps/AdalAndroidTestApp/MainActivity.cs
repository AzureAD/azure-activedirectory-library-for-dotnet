//----------------------------------------------------------------------
//
// Copyright (c) Microsoft Corporation.
// All rights reserved.
//
// This code is licensed under the MIT License.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files(the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions :
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Widget;
using Android.OS;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Text;

namespace AdalAndroidTestApp
{
    [Activity(Label = "AdalAndroidTestApp", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        // An app configured with 2 resources. Note that @microsoft accounts will require consent, but other AADs will work
        private const string clientId = "9379b42e-cd73-43b1-a0c3-51c5abf569eb";
        // This Uri is Android specific, you may have to create your own app to add another one
        private const string redirectUriBroker = "msauth://adalandroidtestapp.adalandroidtestapp/CG0m9vSjvFOspGPjc3TLEZnLHbc=";
        private const string redirectUriNonBroker = "msal9379b42e-cd73-43b1-a0c3-51c5abf569eb://auth";

        private const string resource1 = "https://graph.windows.net";
        private const string resource2 = "https://graph.microsoft.com";

        private UITextView _accessTokenTextView;


        AuthenticationContext _ctx = new AuthenticationContext("https://login.microsoftonline.com/common/");
        string _userName = null;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            Button acquireTokenInteractiveButton = FindViewById<Button>(Resource.Id.acquireTokenInteractiveButton);
            acquireTokenInteractiveButton.Click += acquireTokenInteractiveButton_Click;

            Button acquireTokenSilentButton = FindViewById<Button>(Resource.Id.acquireTokenSilentButton);
            acquireTokenSilentButton.Click += acquireTokenSilentButton_Click;

            Button clearCacheButton = FindViewById<Button>(Resource.Id.clearCacheButton);
            clearCacheButton.Click += clearCacheButton_Click;

            _accessTokenTextView = new UITextView(this, FindViewById<TextView>(Resource.Id.accessTokenTextView));

            // Logging
            LoggerCallbackHandler.PiiLoggingEnabled = true;
            LoggerCallbackHandler.LogCallback = ((lvl, msg, isPii) =>
            {
                string messgeToLog = $"[{lvl}][{isPii}]: {msg}";
                Console.WriteLine(messgeToLog);
            });
        }

        private string GetResource()
        {
            RadioGroup radioGroup = FindViewById<RadioGroup>(Resource.Id.radioGroupResource);
            RadioButton radioButton = FindViewById<RadioButton>(radioGroup.CheckedRadioButtonId);

            if (radioButton.Id == Resource.Id.radioButtonR1)
            {
                return resource1;
            }
            else if (radioButton.Id == Resource.Id.radioButtonR2)
            {
                return resource2;
            }

            throw new NotImplementedException("oh noes");
        }

        private Uri GetRedirectUri()
        {
            return UseBroker() ? 
                new Uri(redirectUriBroker) : 
                new Uri(redirectUriNonBroker);
        }

        private bool UseBroker()
        {
            Switch brokerSwitch = FindViewById<Switch>(Resource.Id.switchUseBroker);
            return brokerSwitch.Checked;
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            AuthenticationAgentContinuationHelper.SetAuthenticationAgentContinuationEventArgs(requestCode, resultCode,
                data);
        }


        private async void acquireTokenSilentButton_Click(object sender, EventArgs e)
        {
            string resource = GetResource();

            foreach (var item in _ctx.TokenCache.ReadItems())
            {
                Console.WriteLine(item.Resource);
            }

            if (String.IsNullOrEmpty(_userName))
            {
                _accessTokenTextView.Text = "Call will fail because there is no username";
                return;
            }

            _accessTokenTextView.Text = string.Empty;
            string value = null;
            try
            {
                var userId = new UserIdentifier(_userName, UserIdentifierType.OptionalDisplayableId);

                AuthenticationResult result = await _ctx
                    .AcquireTokenSilentAsync(
                    resource,
                    clientId,
                    userId,
                    new PlatformParameters(this, UseBroker())).ConfigureAwait(true);
                value = result.AccessToken;
            }
            catch (Java.Lang.Exception ex)
            {
                throw new InvalidOperationException(ex.Message + "\n" + ex.StackTrace);
            }
            catch (Exception exc)
            {
                value = exc.Message;
            }


            _accessTokenTextView.Text = value;
        }

        private async void acquireTokenInteractiveButton_Click(object sender, EventArgs e)
        {
            string resource = GetResource();


            var x = _ctx.TokenCache.Count;
            foreach (var item in _ctx.TokenCache.ReadItems())
            {
                Console.WriteLine(item.Resource);
            }

            _accessTokenTextView.Text = string.Empty;
            string value = null;
            try
            {
                AuthenticationResult result = await _ctx
                    .AcquireTokenAsync(
                    resource,
                    clientId,
                    GetRedirectUri(),
                    new PlatformParameters(this, UseBroker())).ConfigureAwait(true);
                value = result.AccessToken;

                _userName = result.UserInfo.DisplayableId;
            }
            catch (Java.Lang.Exception ex)
            {
                throw new InvalidOperationException(ex.Message + "\n" + ex.StackTrace);
            }
            catch (Exception exc)
            {
                value = exc.Message + x;
            }

            _accessTokenTextView.Text = value;
        }

        private async void clearCacheButton_Click(object sender, EventArgs e)
        {
            await Task.Factory.StartNew(() =>
            {
                TokenCache.DefaultShared.Clear();
                _accessTokenTextView.Text = "Cache cleared";
            });
        }
    }
}