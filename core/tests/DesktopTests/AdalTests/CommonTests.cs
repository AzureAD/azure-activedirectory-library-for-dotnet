using Microsoft.Identity.AutomationTests.Configuration;
using Microsoft.Identity.AutomationTests.Model;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using Test.Microsoft.Identity.LabInfrastructure;

namespace Microsoft.Identity.AutomationTests
{
    public class CommonTests
    {
        private readonly IAutomationTestAppController _testAppController;
        private readonly ResponseValidator _responseValidator;
        private readonly Logger _logger;

        /// <summary>
        /// Flag to indicate whether the tests should use an interactive or non-interactive
        /// flow when seeding (pre-populating) the cache
        /// </summary>
        private readonly bool _seedCacheInteractively;

        public CommonTests(Logger logger, IAutomationTestAppController testAppController, bool seedCacheInteractively)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (testAppController == null)
            {
                throw new ArgumentNullException(nameof(testAppController));
            }

            _testAppController = testAppController;
            _responseValidator = new ResponseValidator(logger);
            _logger = logger;
            _seedCacheInteractively = seedCacheInteractively;

            _testAppController.ClearCache();
        }

        public void AcquireTokenTest(
            ResourceType resourceType,
            ApplicationType application,
            UserQueryParameters userAttributes)
        {
            var user = LabUserHelper.GetLabUserData(userAttributes).User;

            var authRequest = new AuthenticationRequest
            {
                ApplicationType = application,
                ResourceType = resourceType,
                BrokerType = BrokerType.None,
                User = user,
                PromptBehavior = PromptBehavior.Always
            };

            var response = _testAppController.ExecuteAcquireTokenInteractiveFlow(authRequest);
            _responseValidator.AssertIsValid(authRequest, response);

            var readCacheResponse = _testAppController.ReadCache();
            Assert.AreNotEqual(0, readCacheResponse.GetResponseAsDictionary().Any(), "The cache should not be empty");
        }

        public void AcquireTokenTestWithPromptAuto(
            ResourceType resourceType,
            ApplicationType application,
            UserQueryParameters userAttributes)
        {
            var user = LabUserHelper.GetLabUserData(userAttributes).User;

            // First call with prompt auto should be interactive - no cache hit
            var authRequest = new AuthenticationRequest
            {
                ApplicationType = application,
                ResourceType = resourceType,
                BrokerType = BrokerType.None,
                User = user,
                PromptBehavior = PromptBehavior.Auto
            };
            var response1 = _testAppController.ExecuteAcquireTokenInteractiveFlow(authRequest);

            // Second call should just come back silently
            var response2 = _testAppController.ExecuteAcquireTokenNonInteractiveFlow(authRequest);

            // Validate
            _responseValidator.AssertUniqueIdsAreEqual(response1.GetResponseAsDictionary(), response2.GetResponseAsDictionary());
            _responseValidator.AssertAccessTokensAreEqual(response1.GetResponseAsDictionary(), response2.GetResponseAsDictionary());
        }

        public void AcquireTokenInteractiveWithPromptAlways_NoHint(
            ResourceType resourceType,
            ApplicationType application,
            UserQueryParameters userAttributes)
        {
            var user = LabUserHelper.GetLabUserData(userAttributes).User;

            // Seed the cache with a valid token
            var response1 = SeedCache(resourceType, application, user);

            
            // Call acquireToken with PROMPT_ALWAYS without login hint, new AT returned
            var request2 = new AuthenticationRequest
            {
                ApplicationType = application,
                ResourceType = resourceType,
                BrokerType = BrokerType.None,
                User = user,
                PromptBehavior = PromptBehavior.Always
            };
            var response2 = _testAppController.ExecuteAcquireTokenInteractiveFlow(request2);
            _responseValidator.AssertIsValid(resourceType, response2.GetResponseAsDictionary());

            // Validate
            _responseValidator.AssertAccessTokensAreNotEqual(response1.GetResponseAsDictionary(), response2.GetResponseAsDictionary());
        }

        public void AcquireTokenInteractiveWithPromptAlways_WithHint(
            ResourceType resourceType,
            ApplicationType application,
            UserQueryParameters userAttributes)
        {
            var user = LabUserHelper.GetLabUserData(userAttributes).User;
        
            // Add PROMPT_ALWAYS and a login hint
            var request1 = new AuthenticationRequest
            {
                ApplicationType = application,
                ResourceType = resourceType,
                BrokerType = BrokerType.None,
                User = user,
                PromptBehavior = PromptBehavior.Always,
                AdditionalInfo = new Dictionary<string, string>
                {
                    {"user_identifier", user.Upn},
                    {"user_identifier_type", "optional_displayable"}
                }
            };
            
            var response1 = _testAppController.ExecuteAcquireTokenInteractiveFlow(request1);
            _responseValidator.AssertIsValid(resourceType, response1.GetResponseAsDictionary());

            var response2 = _testAppController.ExecuteAcquireTokenInteractiveFlow(request1);
            _responseValidator.AssertIsValid(resourceType, response2.GetResponseAsDictionary());

            // Validate
            _responseValidator.AssertAccessTokensAreNotEqual(response1.GetResponseAsDictionary(), response2.GetResponseAsDictionary());
        }

        public void AcquireTokenSilentWithValidAccessTokenTest(
            ResourceType resourceType,
            ApplicationType application,
            UserQueryParameters userAttributes)
        {
            var user = LabUserHelper.GetLabUserData(userAttributes).User;

            // Seed the cache with a valid token
            var response1 = SeedCache(resourceType, application, user);

            // Issue silent acquireToken call with no specified user
            // TODO: check - "no specified user": but the request does include a user
            var authRequest = new AuthenticationRequest
            {
                ApplicationType = application,
                ResourceType = resourceType,
                BrokerType = BrokerType.None,
                User = user
            };

            var response2 = _testAppController.ExecuteAcquireTokenSilentFlow(authRequest);
            _responseValidator.AssertIsValid(authRequest, response2);

            _responseValidator.AssertUniqueIdsAreEqual(response1.GetResponseAsDictionary(), response2.GetResponseAsDictionary());
            _responseValidator.AssertAccessTokensAreEqual(response1.GetResponseAsDictionary(), response2.GetResponseAsDictionary());
        }

        public void AcquireTokenSilentCrossTenant(
            ResourceType resourceType,
            ApplicationType application,
            UserQueryParameters userAttributes)
        {
            userAttributes.IsExternalUser = true;
            var user = LabUserHelper.GetLabUserData(userAttributes).User;
            // Seed the cache with a valid token from the home tenant user
             var response1 = SeedCache(resourceType, application, user.HomeUser); 

            // Issue silent acquireToken call on the guest tenant user
            var silentAuthRequestForDifferentTenant = new AuthenticationRequest
            {
                ApplicationType = application,
                Authority = AuthenticationRequest.CreateSpecificAuthorityUriFromGuid(user.CurrentTenantId),
                ResourceType = resourceType,
                BrokerType = BrokerType.None,
                User = user
            };
            var silentAuthResponse = _testAppController.ExecuteAcquireTokenSilentFlow(silentAuthRequestForDifferentTenant);
            _responseValidator.AssertIsValid(silentAuthRequestForDifferentTenant, silentAuthResponse);

            // The user should not be the same (home OID, then guest OID), but the access token should be different
            _responseValidator.AssertUniqueIdsAreNotEqual(response1.GetResponseAsDictionary(), silentAuthResponse.GetResponseAsDictionary());
            _responseValidator.AssertAccessTokensAreNotEqual(response1.GetResponseAsDictionary(), silentAuthResponse.GetResponseAsDictionary());
        }

        public void AcquireTokenSilentByRefreshTokenTest(
            ResourceType resourceType,
            ApplicationType application,
            UserQueryParameters userAttributes)
        {
            var user = LabUserHelper.GetLabUserData(userAttributes).User;

            // Seed the cache with a valid token, but expire the access token
            var response1 = SeedCache(resourceType, application, user,
                expireAccessToken: true);


            // Issue silent acquireToken call with a specified username
            var request2 = new AuthenticationRequest
            {
                ApplicationType = application,
                ResourceType = resourceType,
                BrokerType = BrokerType.None,
                User = user,
                AdditionalInfo = new Dictionary<string, string>
                    {{"user_identifier", user.Upn}}
            };
            var response2 = _testAppController.ExecuteAcquireTokenSilentFlow(request2);
            _responseValidator.AssertIsValid(request2, response2);

            // The user should be the same, but the access token should be different
            _responseValidator.AssertUniqueIdsAreEqual(response1.GetResponseAsDictionary(), response2.GetResponseAsDictionary());
            _responseValidator.AssertAccessTokensAreNotEqual(response1.GetResponseAsDictionary(), response2.GetResponseAsDictionary());
        }

        public void AcquireTokenSilentWithRefreshTokenRejectedTest(
            ResourceType resourceType,
            ApplicationType application,
            UserQueryParameters userAttributes)
        {
            var user = LabUserHelper.GetLabUserData(userAttributes).User;

            // Seed the cache with a valid token
            SeedCache(resourceType, application, user,
                expireRefreshToken: true);

            // Silent auth should fail with an error
            var authRequest = new AuthenticationRequest
            {
                ApplicationType = application,
                ResourceType = resourceType,
                BrokerType = BrokerType.None,
                User = user
            };
            var response2 = _testAppController.ExecuteAcquireTokenSilentFlow(authRequest);

            Assert.IsFalse(response2.IsSuccess, "Expected silent authentication to fail");
            Assert.IsNotNull(response2.GetErrorMessage());
            // The below is in response to platform-specific error messages.
            // Android: "AUTH_REFRESH_FAILED_PROMPT_NOT_ALLOWED"
            // iOS: "AD_ERROR_SERVER_REFRESH_TOKEN_REJECTED"
            // Desktop: "Failed to refresh access token"
            Assert.IsTrue(response2.GetErrorMessage().ToUpper().Contains("REFRESH"));
        }

        public void AcquireTokenSilentWithFamilyRefreshTokenTest(
            ResourceType resourceType,
            ApplicationType app1,
            ApplicationType app2,
            UserQueryParameters userAttributes)
        {
            var user = LabUserHelper.GetLabUserData(userAttributes).User;

            // Seed the cache with a valid token

            // Get a token for the first application, interactively.
            var request1 = new AuthenticationRequest
            {
                ApplicationType = app1,
                ResourceType = resourceType,
                BrokerType = BrokerType.None,
                User = user,
                AdditionalInfo = new Dictionary<string, string>
                    {{"redirect_uri", "urn:ietf:wg:oauth:2.0:oob"}} // TODO: Move this into applications.json
            };
            var response1 = _testAppController.ExecuteAcquireTokenInteractiveFlow(request1);
            _responseValidator.AssertIsValid(request1, response1);

            // Get a token for the second application, silently. 
            // Use the unique id provided in the auth response from the first application.
            var request2 = new AuthenticationRequest
            {
                ApplicationType = app2,
                ResourceType = resourceType,
                BrokerType = BrokerType.None,
                User = user,
                AdditionalInfo = new Dictionary<string, string>
                    {{"unique_id", response1.GetUniqueId()}}
            };
            var response2 = _testAppController.ExecuteAcquireTokenSilentFlow(request2);
            _responseValidator.AssertIsValid(request2, response2);
        }

        public void AcquireTokenSilentWithMultiResourceRefreshTokenFallbackTest(
            ResourceType resourceType,
            ApplicationType application,
            UserQueryParameters userAttributes)
        {
            var user = LabUserHelper.GetLabUserData(userAttributes).User;

            // Seed the cache with a valid token

            // Get a token for the first application, interactively.
            var request1 = new AuthenticationRequest
            {
                ApplicationType = application,
                ResourceType = resourceType,
                BrokerType = BrokerType.None,
                User = user
            };
            var response1 = _testAppController.ExecuteAcquireTokenInteractiveFlow(request1);
            _responseValidator.AssertIsValid(request1, response1);

            // Invalidate the FRT and expire the AT. This forces the use of the MRRT as a fallback.
            _testAppController.ExpireUserTokens(request1, TokenType.FamilyRefreshToken);

            // Get a token silently. This should use the MRRT to pick up a new AT and a new FRT.
            var request2 = new AuthenticationRequest
            {
                ApplicationType = application,
                ResourceType = resourceType,
                BrokerType = BrokerType.None,
                User = user,
                AdditionalInfo = new Dictionary<string, string>
                    {{"unique_id", response1.GetUniqueId()}}
            };
            var response2 = _testAppController.ExecuteAcquireTokenSilentFlow(request2);
            _responseValidator.AssertIsValid(request2, response2);
        }

        public void AcquireTokenSilentWithExpiredRefreshTokenInCache(
            ResourceType resourceType,
            ApplicationType application,
            UserQueryParameters userAttributes)
        {
            var user = LabUserHelper.GetLabUserData(userAttributes).User;

            // Seed the cache with a valid token but expire both tokens
            SeedCache(resourceType, application, user,
                expireAccessToken: true,
                expireRefreshToken: true);
            
            // Check Aquire token Silent
            var request2 = new AuthenticationRequest
            {
                ApplicationType = application,
                ResourceType = resourceType,
                BrokerType = BrokerType.None,
                User = user,
                AdditionalInfo = new Dictionary<string, string>
                    {{"user_identifier", user.Upn}}
            };
            var response2 = _testAppController.ExecuteAcquireTokenSilentFlow(request2);

            Assert.IsFalse(response2.IsSuccess, "Expected silent authentication to fail");
            Assert.AreEqual("Failed to refresh access token", response2.GetErrorMessage());

            // Read cache
            var readCacheResponse = _testAppController.ReadCache();
            Assert.AreNotEqual(0, readCacheResponse.GetResponseAsDictionary().Any(), "The cache should not be empty");

            // If possible, check the expire time has passed
            Assert.IsTrue(!readCacheResponse.HasExpireTime() || readCacheResponse.GetExpireTime() < DateTime.UtcNow, "Expected access token to have expired.");
        }

        public void AcquireTokenTestWithPromptNever(
            ResourceType resourceType,
            ApplicationType application,
            UserQueryParameters userAttributes)
        {
            var user = LabUserHelper.GetLabUserData(userAttributes).User;

            var response1 = SeedCache(resourceType, application, user);


            // Now testing PROMPT_NEVER, this should come back silently
            var silentRequest = new AuthenticationRequest
            {                      
                ApplicationType = application,
                ResourceType = resourceType,
                BrokerType = BrokerType.None,
                User = user,
                PromptBehavior = PromptBehavior.Never
            };
            var response2 = _testAppController.ExecuteAcquireTokenNonInteractiveFlow(silentRequest);
            _responseValidator.AssertIsValid(silentRequest, response2);

            // Although the second call came back silently we expect the tokens should be different
            _responseValidator.AssertUniqueIdsAreEqual(response1.GetResponseAsDictionary(), response2.GetResponseAsDictionary());
            _responseValidator.AssertAccessTokensAreNotEqual(response1.GetResponseAsDictionary(), response2.GetResponseAsDictionary());
        }

        private AuthenticationResponse SeedCache(ResourceType resourceType,
            ApplicationType application,
            LabUser user,
            bool expireAccessToken = false,
            bool expireRefreshToken = false)
        {

            AuthenticationResponse authResponse;
            var request = new AuthenticationRequest
            {
                ApplicationType = application,
                ResourceType = resourceType,
                BrokerType = BrokerType.None,
                User = user
            };

            if (_seedCacheInteractively)
            {
                // Run through the interactive auth flow for test account
                _logger.LogInfo("TEST SETUP: seeding cache interactively...");

                request.PromptBehavior = PromptBehavior.Always;
                authResponse = _testAppController.ExecuteAcquireTokenInteractiveFlow(request);
            }
            else
            {
                // There should not be any prompt and it should work as silent
                _logger.LogInfo("TEST SETUP: seeding cache non-interactively...");
                request.AdditionalInfo = new Dictionary<string, string>
                {
                    {"user_identifier", user.Upn},
                    {"password", LabUserHelper.GetUserPassword(user)}
                };
                authResponse = _testAppController.ExecuteAcquireTokenNonInteractiveFlow(request);
            }

            if (!authResponse.IsSuccess)
            {
                _logger.LogInfo($"  User name: {user.Upn}");
                _logger.LogInfo($"  User id: {user.ObjectId}");
                _logger.LogInfo($"  Authentication response: {authResponse.Response}");
                _logger.LogInfo($"  Authentication logs: {authResponse.ResultLogs}");
                Assert.Fail($"Test initialisation error: failed to seed the cache for user {user.Upn}.\n\nAuthentication response: {authResponse.Response}\n\nSee the test output for more information.");
            }
            _responseValidator.AssertIsExpectedUser(user, authResponse);

            if (expireAccessToken)
            {
                _logger.LogInfo("TEST SETUP: expiring access token...");
                _testAppController.ExpireUserTokens(request, TokenType.AccessToken);

                var readCacheResponse = _testAppController.ReadCache();
                Assert.AreNotEqual(0, readCacheResponse.GetResponseAsDictionary().Any(), "The cache should not be empty");

                // Not all test apps return the expire time so we can only check it if it is present
                if (readCacheResponse.HasExpireTime())
                {
                    _logger.LogInfo("TEST SETUP: checking the access token has expired...");
                    Assert.IsTrue(readCacheResponse.GetExpireTime() < DateTime.UtcNow, $"Expected access token to have expired. Expire time: {readCacheResponse.GetExpireTime()}");
                }
                else
                {
                    _logger.LogInfo("TEST SETUP: unable to check the access token has expired - the test app did not return the expiry time");
                }
            }

            if (expireRefreshToken)
            {
                _logger.LogInfo("TEST SETUP: invalidating refresh token...");
                _testAppController.ExpireUserTokens(request, TokenType.RefreshToken);

                var readCacheResponse = _testAppController.ReadCache();
                Assert.AreNotEqual(0, readCacheResponse.GetResponseAsDictionary().Any(), "The cache should not be empty");
                // TODO: different test apps invalidate the refresh token in different ways so there isn't a simple way to check the token has been invalidated
                //Assert.AreEqual("bad_refresh_token", readCacheResponse.GetRefreshToken(), "There should be an invalid refresh token in the cache");
            }

            _logger.LogInfo("TEST SETUP: cache seeded successfully");
            return authResponse;
        }
    }
}
