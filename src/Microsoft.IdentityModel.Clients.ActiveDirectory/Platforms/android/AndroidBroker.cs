//------------------------------------------------------------------------------
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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Android.Accounts;
using Android.App;
using Android.Content;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.Flows;
using Microsoft.Identity.Core;
using Microsoft.Identity.Core.Cache;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.OAuth2;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.Helpers;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.Broker;

namespace Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.Platform
{
    [Android.Runtime.Preserve(AllMembers = true)]
    internal class AndroidBroker : IBroker
    {
        private static SemaphoreSlim readyForResponse = null;
        private static AdalResultWrapper resultEx = null;

        private readonly AndroidBrokerProxy _brokerProxy;
        private readonly ICoreLogger _logger;

        public AndroidBroker(ICoreLogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _brokerProxy =  new AndroidBrokerProxy(Application.Context, logger);
        }

        public IPlatformParameters PlatformParameters { get; set; }


        public bool CanInvokeBroker
        {
            get
            {                
                bool canInvoke =  WillUseBroker() && _brokerProxy.CanSwitchToBroker();
                _logger.Verbose("Can invoke broker? " + canInvoke);
                return canInvoke;
            }
        }

        public async Task<AdalResultWrapper> AcquireTokenUsingBrokerAsync(IDictionary<string, string> brokerPayload)
        {
            resultEx = null;
            readyForResponse = new SemaphoreSlim(0);
            try
            {
                await Task.Run(() => AcquireTokenInternal(brokerPayload)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.ErrorPii(ex);
                throw;
            }
            await readyForResponse.WaitAsync().ConfigureAwait(false);
            return resultEx;
        }

        private void AcquireTokenInternal(IDictionary<string, string> brokerPayload)
        {
            if (brokerPayload.ContainsKey(BrokerParameter.BrokerInstallUrl))
            {
                _logger.Info("Android Broker - broker payload contains install url");

                string url = brokerPayload[BrokerParameter.BrokerInstallUrl];
                Uri uri = new Uri(url);
                string query = uri.Query;
                if (query.StartsWith("?", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Substring(1);
                }

                Dictionary<string, string> keyPair = EncodingHelper.ParseKeyValueList(query, '&', true, false, null);

                PlatformParameters pp = PlatformParameters as PlatformParameters;
                var appLink = keyPair["app_link"];
                _logger.Info("Android Broker - Starting ActionView activity to " + appLink);
                pp.CallerActivity.StartActivity(new Intent(Intent.ActionView, Android.Net.Uri.Parse(appLink)));

                throw new AdalException(AdalErrorAndroidEx.BrokerApplicationRequired, AdalErrorMessageAndroidEx.BrokerApplicationRequired);
            }

            Context mContext = Application.Context;
            AuthenticationRequest request = new AuthenticationRequest(brokerPayload);
            PlatformParameters platformParams = PlatformParameters as PlatformParameters;

            // BROKER flow intercepts here
            // cache and refresh call happens through the authenticator service
            if (_brokerProxy.VerifyUser(request.LoginHint,
                request.UserId))
            {

                request.BrokerAccountName = request.LoginHint;
                _logger.InfoPii(
                    "It switched to broker for context: " + mContext.PackageName + " login hint: " + request.BrokerAccountName,
                    "It switched to broker for context");                

                // Don't send background request, if prompt flag is always or
                // refresh_session
                bool hasAccountNameOrUserId = !string.IsNullOrEmpty(request.BrokerAccountName) || !string.IsNullOrEmpty(request.UserId);
                if (string.IsNullOrEmpty(request.Claims) && hasAccountNameOrUserId)
                {
                    _logger.Verbose("User is specified for background token request");
                    resultEx = _brokerProxy.GetAuthTokenInBackground(request, platformParams.CallerActivity);
                }
                else
                {
                    _logger.Verbose("User is not specified for background token request");
                }

                if (resultEx != null && resultEx.Result != null && !string.IsNullOrEmpty(resultEx.Result.AccessToken))
                {
                    _logger.Verbose("Token is returned from background call");
                    readyForResponse.Release();
                    return;
                }

                // Launch broker activity
                // if cache and refresh request is not handled.
                // Initial request to authenticator needs to launch activity to
                // record calling uid for the account. This happens for Prompt auto
                // or always behavior.
                _logger.Verbose("Token is not returned from backgroud call");

                // Only happens with callback since silent call does not show UI
                _logger.Verbose("Launch activity for Authenticator");

                _logger.Verbose("Starting Authentication Activity");

                if (resultEx == null)
                {
                    _logger.Verbose("Initial request to authenticator");
                    // Log the initial request but not force a prompt
                }

                if (brokerPayload.ContainsKey(BrokerParameter.SilentBrokerFlow))
                {
                    _logger.Error("Can't invoke the broker in interactive mode because this is a silent flow");
                    throw new AdalSilentTokenAcquisitionException();
                }

                // onActivityResult will receive the response
                // Activity needs to launch to record calling app for this
                // account
                Intent brokerIntent = _brokerProxy.GetIntentForBrokerActivity(request, platformParams.CallerActivity);
                if (brokerIntent != null)
                {
                    try
                    {
                        _logger.Verbose(
                            "Calling activity pid:" + Android.OS.Process.MyPid()
                            + " tid:" + Android.OS.Process.MyTid() + "uid:"
                            + Android.OS.Process.MyUid());

                        platformParams.CallerActivity.StartActivityForResult(brokerIntent, 1001);
                    }
                    catch (ActivityNotFoundException e)
                    {
                        _logger.ErrorPii(e);
                    }
                }
            }
            else
            {
                throw new AdalException(AdalErrorAndroidEx.NoBrokerAccountFound, "Add requested account as a Workplace account via Settings->Accounts or set UseBroker=true.");
            }
        }

        internal static void SetBrokerResult(Intent data, int resultCode)
        {
            if (resultCode != BrokerResponseCode.ResponseReceived)
            {
                resultEx = new AdalResultWrapper
                {
                    Exception =
                        new AdalException(data.GetStringExtra(BrokerConstants.ResponseErrorCode),
                            data.GetStringExtra(BrokerConstants.ResponseErrorMessage))
                };
            }
            else
            {
                var tokenResponse = new TokenResponse
                {
                    Authority = data.GetStringExtra(BrokerConstants.AccountAuthority),
                    AccessToken = data.GetStringExtra(BrokerConstants.AccountAccessToken),
                    IdTokenString = data.GetStringExtra(BrokerConstants.AccountIdToken),
                    TokenType = "Bearer",
                    ExpiresOn = data.GetLongExtra(BrokerConstants.AccountExpireDate, 0)
                };

                resultEx = tokenResponse.GetResult(AndroidBrokerProxy.ConvertFromTimeT(tokenResponse.ExpiresOn),
                    AndroidBrokerProxy.ConvertFromTimeT(tokenResponse.ExpiresOn));
            }

            readyForResponse.Release();
        }

        
        private bool WillUseBroker()
        {
            PlatformParameters pp = PlatformParameters as PlatformParameters;
            bool useBroker = pp?.UseBroker ?? false;

            _logger.Verbose("Is Android Broker use configured via PlatformParamters? " + useBroker);

            return useBroker;
        }
    }
}
