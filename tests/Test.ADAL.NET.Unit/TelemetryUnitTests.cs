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
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.ADAL.NET.Unit.Mocks;

namespace Test.ADAL.NET.Unit
{
    [TestClass]
    public class TelemetryUnitTests
    {
        private PlatformParameters platformParameters;

        [TestInitialize]
        public void Initialize()
        {
            HttpMessageHandlerFactory.ClearMockHandlers();
            platformParameters = new PlatformParameters(PromptBehavior.Auto);
        }

        [TestMethod]
        [Description("Telemetry tests Default Dispatcher")]
        public void TelemetryDefaultDispatcher()
        {
            Telemetry telemetry = Telemetry.GetInstance();
            Assert.IsNotNull(telemetry);

            DispatcherImplement dispatcher = new DispatcherImplement();
            telemetry.RegisterDispatcher(dispatcher, false);
            dispatcher.clear();
            string requestIDThree = telemetry.CreateRequestId();
            telemetry.StartEvent(requestIDThree, "event_3");
            DefaultEvent testDefaultEvent3 = new DefaultEvent();
            Assert.IsNotNull(DefaultEvent.ApplicationVersion);
            telemetry.StopEvent(requestIDThree, testDefaultEvent3, "event_3");

            telemetry.StartEvent(requestIDThree, "event_4");
            DefaultEvent testDefaultEvent4 = new DefaultEvent();
            Assert.IsNotNull(DefaultEvent.ApplicationVersion);
            telemetry.StopEvent(requestIDThree, testDefaultEvent4, "event_4");

            telemetry.StartEvent(requestIDThree, "event_5");
            DefaultEvent testDefaultEvent5 = new DefaultEvent();
            Assert.IsNotNull(DefaultEvent.ApplicationVersion);
            telemetry.StopEvent(requestIDThree, testDefaultEvent5, "event_5");
            telemetry.Flush(requestIDThree);
            Assert.AreEqual(dispatcher.Count, 3);
        }


        [TestMethod]
        [Description("Test case for checking CacheEvent")]
        public void TelemetryCacheEvent()
        {
            Telemetry telemetry = Telemetry.GetInstance();
            Assert.IsNotNull(telemetry);

            DispatcherImplement dispatcher = new DispatcherImplement();
            telemetry.RegisterDispatcher(dispatcher, true);
            dispatcher.clear();
            string requestIDThree = telemetry.CreateRequestId();
            telemetry.StartEvent(requestIDThree, "cache_lookup");
            CacheEvent testDefaultEvent = new CacheEvent();
            Assert.IsNotNull(DefaultEvent.ApplicationVersion);
            telemetry.StopEvent(requestIDThree, testDefaultEvent, "cache_lookup");
            telemetry.Flush(requestIDThree);
            Assert.AreEqual(dispatcher.Count, 1);

            bool result = dispatcher.CacheTelemetryValidator();

            Assert.IsTrue(result);
        }

        [TestMethod]
        [Description("Test case for checking APIEvent")]
        public void TelemetryApiEvent()
        {
            Telemetry telemetry = Telemetry.GetInstance();
            Assert.IsNotNull(telemetry);

            DispatcherImplement dispatcher = new DispatcherImplement();
            telemetry.RegisterDispatcher(dispatcher, true);
            dispatcher.clear();
            string requestIDThree = telemetry.CreateRequestId();
            telemetry.StartEvent(requestIDThree, "api_event");
            Authenticator authenticator = new Authenticator(TestConstants.DefaultAuthorityCommonTenant, true);
            UserInfo userinfo = new UserInfo();
            userinfo.UniqueId = "uniqueid";
            userinfo.DisplayableId = "displayableid";
            ApiEvent testDefaultEvent = new ApiEvent(authenticator, userinfo, "tenantId", "3");
            Assert.IsNotNull(DefaultEvent.ApplicationVersion);
            telemetry.StopEvent(requestIDThree, testDefaultEvent, "cache_lookup");
            telemetry.Flush(requestIDThree);
            Assert.AreEqual(dispatcher.Count, 0);

            bool result = dispatcher.ApiTelemetryValidator();

            Assert.IsTrue(result);
        }

        [TestMethod]
        [Description("Telemetry tests Aggregate Dispatcher for a single event in requestID")]
        public void TelemetryAggregateDispatcherSingleEventRequestID()
        {
            Telemetry telemetry = Telemetry.GetInstance();
            Assert.IsNotNull(telemetry);

            DispatcherImplement dispatcher = new DispatcherImplement();
            telemetry.RegisterDispatcher(dispatcher, true);
            dispatcher.clear();
            string requestIDThree = telemetry.CreateRequestId();
            telemetry.StartEvent(requestIDThree, "event_3");
            DefaultEvent testDefaultEvent = new DefaultEvent();
            Assert.IsNotNull(DefaultEvent.ApplicationVersion);
            telemetry.StopEvent(requestIDThree, testDefaultEvent, "event_3");
            telemetry.Flush(requestIDThree);
            Assert.AreEqual(dispatcher.Count, 1);
        }

        [TestMethod]
        [Description("Telemetry tests for Aggregate Dispatcher for multiple events in requestID")]
        public void TelemetryAggregateDispatcherMultipleEventsRequestId()
        {
            Telemetry telemetry = Telemetry.GetInstance();
            Assert.IsNotNull(telemetry);

            DispatcherImplement dispatcher = new DispatcherImplement();
            telemetry.RegisterDispatcher(dispatcher, true);
            dispatcher.clear();
            string requestIDThree = telemetry.CreateRequestId();
            telemetry.StartEvent(requestIDThree, "event_3");
            DefaultEvent testDefaultEvent3 = new DefaultEvent();
            Assert.IsNotNull(DefaultEvent.ApplicationVersion);
            telemetry.StopEvent(requestIDThree, testDefaultEvent3, "event_3");

            telemetry.StartEvent(requestIDThree, "event_4");
            DefaultEvent testDefaultEvent4 = new DefaultEvent();
            Assert.IsNotNull(DefaultEvent.ApplicationVersion);
            telemetry.StopEvent(requestIDThree, testDefaultEvent4, "event_4");

            telemetry.StartEvent(requestIDThree, "event_5");
            DefaultEvent testDefaultEvent5 = new DefaultEvent();
            Assert.IsNotNull(DefaultEvent.ApplicationVersion);
            telemetry.StopEvent(requestIDThree, testDefaultEvent5, "event_5");
            telemetry.Flush(requestIDThree);
            Assert.AreEqual(dispatcher.Count, 1);
        }

        [TestMethod]
        [Description("Test for telemetry event in acquireToken prompt never")]
        [TestCategory("AdalDotNet")]
        public async Task TelemetryAcquireTokenTestPromptNever()
        {
            MockHelpers.ConfigureMockWebUI(new AuthorizationResult(AuthorizationStatus.Success,
                TestConstants.DefaultRedirectUri + "?code=some-code"));
            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler
            {
                Method = HttpMethod.Post,
                ResponseMessage = MockHelpers.CreateSuccessTokenResponseMessage()
            });

            Telemetry telemetry = Telemetry.GetInstance();
            DispatcherImplement dispatcher = new DispatcherImplement();
            telemetry.RegisterDispatcher(dispatcher, false);


            AuthenticationContext context = new AuthenticationContext(TestConstants.DefaultAuthorityCommonTenant, true);
            AuthenticationResult result =
                await
                    context.AcquireTokenAsync(TestConstants.DefaultResource, TestConstants.DefaultClientId,
                        TestConstants.DefaultRedirectUri, new PlatformParameters(PromptBehavior.Never),
                        new UserIdentifier(TestConstants.DefaultDisplayableId, UserIdentifierType.RequiredDisplayableId),
                        "extra=123&abc=xyz");

            Assert.IsNotNull(result);
            Assert.IsTrue(context.Authenticator.Authority.EndsWith("/some-tenant-id/"));
            Assert.AreEqual(result.AccessToken, "some-access-token");
            Assert.IsNotNull(result.UserInfo);
            Assert.AreEqual(result.ExpiresOn, result.ExtendedExpiresOn);
            Assert.AreEqual(TestConstants.DefaultDisplayableId, result.UserInfo.DisplayableId);
            Assert.AreEqual(TestConstants.DefaultUniqueId, result.UserInfo.UniqueId);

            Assert.IsTrue(dispatcher.AcquireTokenTelemetryValidator());
            dispatcher.file();
        }

        [TestMethod]
        [Description("Test for telemetry event in acquireToken prompt refresh session")]
        [TestCategory("AdalDotNet")]
        public async Task TelemetryAcquireTokenTestPromptRefreshSession()
        {
            MockHelpers.ConfigureMockWebUI(new AuthorizationResult(AuthorizationStatus.Success,
                TestConstants.DefaultRedirectUri + "?code=some-code"));
            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler
            {
                Method = HttpMethod.Post,
                ResponseMessage = MockHelpers.CreateSuccessTokenResponseMessage()
            });

            Telemetry telemetry = Telemetry.GetInstance();
            DispatcherImplement dispatcher = new DispatcherImplement();
            telemetry.RegisterDispatcher(dispatcher, false);


            AuthenticationContext context = new AuthenticationContext(TestConstants.DefaultAuthorityCommonTenant, true);
            AuthenticationResult result =
                await
                    context.AcquireTokenAsync(TestConstants.DefaultResource, TestConstants.DefaultClientId,
                        TestConstants.DefaultRedirectUri, new PlatformParameters(PromptBehavior.RefreshSession),
                        new UserIdentifier(TestConstants.DefaultDisplayableId, UserIdentifierType.RequiredDisplayableId),
                        "extra=123&abc=xyz");

            Assert.IsNotNull(result);
            Assert.IsTrue(context.Authenticator.Authority.EndsWith("/some-tenant-id/"));
            Assert.AreEqual(result.AccessToken, "some-access-token");
            Assert.IsNotNull(result.UserInfo);
            Assert.AreEqual(result.ExpiresOn, result.ExtendedExpiresOn);
            Assert.AreEqual(TestConstants.DefaultDisplayableId, result.UserInfo.DisplayableId);
            Assert.AreEqual(TestConstants.DefaultUniqueId, result.UserInfo.UniqueId);

            Assert.IsTrue(dispatcher.AcquireTokenTelemetryValidator());
            dispatcher.file();
        }

        [TestMethod]
        [Description("Negative Test for AcquireToken with invalid resource")]
        public async Task AcquireTokenWithInvalidResourceTestAsyncSilent()
        {
            Telemetry telemetry = Telemetry.GetInstance();
            DispatcherImplement dispatcher = new DispatcherImplement();
            telemetry.RegisterDispatcher(dispatcher, false);

            var context = new AuthenticationContext(TestConstants.DefaultAuthorityCommonTenant, new TokenCache());
            TokenCacheKey key = new TokenCacheKey(TestConstants.DefaultAuthorityHomeTenant,
                TestConstants.DefaultResource, TestConstants.DefaultClientId, TokenSubjectType.User,
                TestConstants.DefaultUniqueId, TestConstants.DefaultDisplayableId);
            context.TokenCache.tokenCacheDictionary[key] = new AuthenticationResultEx
            {
                RefreshToken = "some-rt",
                ResourceInResponse = TestConstants.DefaultResource,
                Result = new AuthenticationResult("Bearer", "some-access-token", DateTimeOffset.UtcNow)
            };

            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler()
            {
                Method = HttpMethod.Post,
                ResponseMessage = MockHelpers.CreateInvalidRequestTokenResponseMessage()
            });

            try
            {
                await context.AcquireTokenSilentAsync("random-resource", TestConstants.DefaultClientId);
            }
            catch (AdalServiceException exc)
            {
                Assert.AreEqual(AdalError.FailedToRefreshToken, exc.ErrorCode);
            }

            Assert.IsTrue(dispatcher.AcquireTokenTelemetryValidatorSilentAPI());
            dispatcher.file();
        }

        [TestMethod]
        [Description("Test for telemetry event in acquireToken with prompt auto")]
        [TestCategory("AdalDotNet")]
        public async Task TelemetryAcquireTokenTestPromptAuto()
        {
            MockHelpers.ConfigureMockWebUI(new AuthorizationResult(AuthorizationStatus.Success,
                TestConstants.DefaultRedirectUri + "?code=some-code"));
            HttpMessageHandlerFactory.AddMockHandler(new MockHttpMessageHandler
            {
                Method = HttpMethod.Post,
                ResponseMessage = MockHelpers.CreateSuccessTokenResponseMessage()
            });

            Telemetry telemetry = Telemetry.GetInstance();
            DispatcherImplement dispatcher = new DispatcherImplement();
            telemetry.RegisterDispatcher(dispatcher, false);


            AuthenticationContext context = new AuthenticationContext(TestConstants.DefaultAuthorityCommonTenant, true);
            AuthenticationResult result =
                await
                    context.AcquireTokenAsync(TestConstants.DefaultResource, TestConstants.DefaultClientId,
                        TestConstants.DefaultRedirectUri, new PlatformParameters(PromptBehavior.Auto),
                        new UserIdentifier(TestConstants.DefaultDisplayableId, UserIdentifierType.RequiredDisplayableId),
                        "extra=123&abc=xyz");

            Assert.IsNotNull(result);
            Assert.IsTrue(context.Authenticator.Authority.EndsWith("/some-tenant-id/"));
            Assert.AreEqual(result.AccessToken, "some-access-token");
            Assert.IsNotNull(result.UserInfo);
            Assert.AreEqual(result.ExpiresOn, result.ExtendedExpiresOn);
            Assert.AreEqual(TestConstants.DefaultDisplayableId, result.UserInfo.DisplayableId);
            Assert.AreEqual(TestConstants.DefaultUniqueId, result.UserInfo.UniqueId);

            Assert.IsTrue(dispatcher.AcquireTokenTelemetryValidator());
            dispatcher.file();
        }

        private class DispatcherImplement : IDispatcher
        {
            private readonly List<List<Tuple<string, string>>> storeList = new List<List<Tuple<string, string>>>();

            public int Count
            {
                get { return storeList.Count; }
            }

            void IDispatcher.DispatchEvent(List<Tuple<string, string>> Event)
            {
                storeList.Add(Event);
            }

            public void clear()
            {
                storeList.Clear();
            }

            public void file()
            {
                using (TextWriter tw = new StreamWriter("test.txt"))
                {
                    foreach (List<Tuple<string, string>> list in storeList)
                    {
                        foreach (Tuple<string, string> tuple in list)
                        {
                            tw.WriteLine(tuple.Item1 + " " + tuple.Item2 + "\r\n");
                        }
                    }
                }
            }

            public bool CacheTelemetryValidator()
            {
                HashSet<string> Cacheitems = new HashSet<string>();
                Cacheitems.Add("event_name");
                Cacheitems.Add("application_name");
                Cacheitems.Add("application_version");
                Cacheitems.Add("x-client-version");
                Cacheitems.Add("x-client-sku");
                Cacheitems.Add("device_id");
                Cacheitems.Add("correlation_id");
                Cacheitems.Add("start_time");
                Cacheitems.Add("end_time");
                Cacheitems.Add("response_time");
                Cacheitems.Add("request_id");

                foreach (List<Tuple<string, string>> list in storeList)
                {
                    foreach (Tuple<string, string> tuple in list)
                    {
                        if (!(Cacheitems.Contains(tuple.Item1) && tuple.Item2 != null && tuple.Item2.Length > 0))
                        {
                            return false;
                        }
                    }
                }
                return true;
            }

            public bool ApiTelemetryValidator()
            {
                HashSet<string> Apiitems = new HashSet<string>();
                Apiitems.Add("event_name");
                Apiitems.Add("application_name");
                Apiitems.Add("application_version");
                Apiitems.Add("sdk_version");
                Apiitems.Add("sdk_platform");
                Apiitems.Add("device_id");
                Apiitems.Add("correlation_id");
                Apiitems.Add("start_time");
                Apiitems.Add("end_time");
                Apiitems.Add("response_time");
                Apiitems.Add("request_id");
                Apiitems.Add("is_deprecated");
                Apiitems.Add("idp");
                Apiitems.Add("displayable_id");
                Apiitems.Add("unique_id");
                Apiitems.Add("authority");
                Apiitems.Add("authority_type");
                Apiitems.Add("validation_status");
                Apiitems.Add("extended_expires_on_setting");
                Apiitems.Add("is_successful");
                Apiitems.Add("user_id");
                Apiitems.Add("tenant_id");
                Apiitems.Add("login_hint");
                Apiitems.Add("api_id");

                foreach (List<Tuple<string, string>> list in storeList)
                {
                    foreach (Tuple<string, string> tuple in list)
                    {
                        if (!(Apiitems.Contains(tuple.Item1) && tuple.Item2 != null && tuple.Item2.Length > 0))
                        {
                            return false;
                        }
                    }
                }
                return true;
            }

            public bool AcquireTokenTelemetryValidator()
            {
                HashSet<string> items = new HashSet<string>();
                items.Add("event_name");
                items.Add("application_name");
                items.Add("application_version");
                items.Add("sdk_version");
                items.Add("sdk_platform");
                items.Add("device_id");
                items.Add("correlation_id");
                items.Add("start_time");
                items.Add("end_time");
                items.Add("response_time");
                items.Add("request_id");
                items.Add("is_deprecated");
                items.Add("idp");
                items.Add("displayable_id");
                items.Add("unique_id");
                items.Add("authority");
                items.Add("authority_type");
                items.Add("validation_status");
                items.Add("extended_expires_on_setting");
                items.Add("is_successful");
                items.Add("user_id");
                items.Add("tenant_id");
                items.Add("broker_app");
                items.Add("broker_version");
                items.Add("login_hint");
                items.Add("api_id");
                items.Add("user_agent");
                items.Add("method");
                items.Add("query_parameters");
                items.Add("response_code");
                items.Add("response_method");
                items.Add("api_version");
                items.Add("token_found");
                items.Add("request_api_version");
                items.Add("token_subject_type");
                items.Add("user_cancel");
                items.Add("redirects_count");
                items.Add("is_MRRT");
                items.Add("is_AT");
                items.Add("extra_query_parameters");

                foreach (List<Tuple<string, string>> list in storeList)
                {
                    foreach (Tuple<string, string> tuple in list)
                    {
                        if ((!(items.Contains(tuple.Item1) && tuple.Item2 != null && tuple.Item2.Length > 0))
                            || ((tuple.Item1.Equals("api_id") && !tuple.Item2.Equals(EventConstants.AcquireTokenAsyncInteractive3))))
                        {
                            return false;
                        }

                        if ((tuple.Item1.Equals("extra_query_parameters") && !tuple.Item2.Equals("extra&abc&")))
                        {
                            return false;
                        }
                    }
                }
                return true;
            }

            public bool AcquireTokenTelemetryValidatorSilentAPI()
            {
                HashSet<string> items = new HashSet<string>();
                items.Add("event_name");
                items.Add("application_name");
                items.Add("application_version");
                items.Add("sdk_version");
                items.Add("sdk_platform");
                items.Add("device_id");
                items.Add("correlation_id");
                items.Add("start_time");
                items.Add("end_time");
                items.Add("response_time");
                items.Add("request_id");
                items.Add("is_deprecated");
                items.Add("idp");
                items.Add("displayable_id");
                items.Add("unique_id");
                items.Add("authority");
                items.Add("authority_type");
                items.Add("validation_status");
                items.Add("extended_expires_on_setting");
                items.Add("is_successful");
                items.Add("user_id");
                items.Add("tenant_id");
                items.Add("broker_app");
                items.Add("broker_version");
                items.Add("login_hint");
                items.Add("api_id");
                items.Add("user_agent");
                items.Add("method");
                items.Add("query_parameters");
                items.Add("response_code");
                items.Add("response_method");
                items.Add("api_version");
                items.Add("token_found");
                items.Add("request_api_version");
                items.Add("token_subject_type");
                items.Add("user_cancel");
                items.Add("redirects_count");
                items.Add("is_MRRT");
                items.Add("is_AT");
                items.Add("extra_query_parameters");

                foreach (List<Tuple<string, string>> list in storeList)
                {
                    foreach (Tuple<string, string> tuple in list)
                    {
                        if ((!(items.Contains(tuple.Item1) && tuple.Item2 != null && tuple.Item2.Length > 0))
                            || ((tuple.Item1.Equals("api_id") && !tuple.Item2.Equals(EventConstants.AcquireTokenSilentAsync1))))
                        {
                            return false;
                        }

                        if ((tuple.Item1.Equals("extra_query_parameters") && !tuple.Item2.Equals("extra&abc&")))
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
        }
    }
}