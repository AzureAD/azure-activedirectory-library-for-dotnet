using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Identity.AutomationTests.Configuration;
using Microsoft.Identity.AutomationTests.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Identity.Labs;

namespace Microsoft.Identity.AutomationTests
{
    public class ResponseValidator
    {
        private readonly Logger _logger;
        private readonly JwtSecurityTokenHandler _tokenHandler;

        public ResponseValidator(Logger logger)
        {
            _logger = logger;
            _tokenHandler = new JwtSecurityTokenHandler();
        }

        public void AssertIsValid(AuthenticationRequest request, AuthenticationResponse response)
        {
            IReadOnlyDictionary<string, object> responseDict = response.GetResponseAsDictionary();
            AssertIsValid(request.ResourceType, responseDict);

            AssertIsExpectedUser(request.User, response);
        }

        public void AssertIsValid(ResourceType resourceType, IReadOnlyDictionary<string, object> authResponse)
        {
            _logger.LogInfo("ResponseValidator: Validating the access token");

            Assert.AreNotEqual(0, authResponse.Count, "The auth response is empty");
            if (authResponse.ContainsKey("error"))
            {
                Assert.Fail("The auth response contains an error: " + authResponse["error"]);
            }
            Assert.IsTrue(authResponse.ContainsKey("access_token"), "The auth response does not contain an access token");
            Assert.IsTrue(authResponse.ContainsKey("tenant_id"), "The auth response does not contain the tenant id");

            string protectedUrl = GetProtectedUrl(resourceType, authResponse["tenant_id"].ToString());
            _logger.LogInfo($"ResponseValidator: Making requests against {protectedUrl}");

            Assert.IsTrue(_tokenHandler.CanReadToken(authResponse["access_token"].ToString()), "The access token is unreadable");

            using (var client = new HttpClient())
            using (var requestWithAuth = new HttpRequestMessage(HttpMethod.Get, protectedUrl))
            using (var requestNoAuth = new HttpRequestMessage(HttpMethod.Get, protectedUrl))
            {
                requestWithAuth.Headers.Add("Authorization", $"Bearer {authResponse["access_token"]}");

                var noAuthResponseTask = client.SendAsync(requestNoAuth);
                var withAuthResposneTask = client.SendAsync(requestWithAuth);

                try
                {
                    Task.WaitAll(noAuthResponseTask, withAuthResposneTask);
                }
                catch (AggregateException ex)
                {
                    _logger.LogError("ResponseValidator: An error occured while validating the access token. ", ex.Flatten());
                    throw;
                }

                _logger.LogInfo("ResponseValidator: Requests completed. Validating response codes");

                Assert.AreEqual(
                    HttpStatusCode.Unauthorized,
                    noAuthResponseTask.Result.StatusCode,
                    "Request without auth header should fail with 401");
                _logger.LogInfo("ResponseValidator: The request without an auth header returned 401");

                var content = withAuthResposneTask.Result.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                Assert.IsTrue(withAuthResposneTask.Result.IsSuccessStatusCode, $"Request with auth header should not fail:{Environment.NewLine}{content}");
                _logger.LogInfo("ResponseValidator: The request with an auth header succeeded");
            }
        }

        public void AssertUniqueIdsAreEqual(IReadOnlyDictionary<string, object> authResponse1,
            IReadOnlyDictionary<string, object> authResponse2)
        {
            Assert.AreEqual(authResponse1["unique_id"].ToString(), authResponse2["unique_id"].ToString(), "Unique ids should be equal.");
        }

        public void AssertUniqueIdsAreNotEqual(IReadOnlyDictionary<string, object> authResponse1,
            IReadOnlyDictionary<string, object> authResponse2)
        {
            Assert.AreNotEqual(authResponse1["unique_id"].ToString(), authResponse2["unique_id"].ToString(), "Unique ids should not be equal.");
        }

        public void AssertIsExpectedUser(IUser user, AuthenticationResponse authResponse)
        {
            Assert.IsNotNull(user, "User should not be null");
            Assert.IsNotNull(authResponse, "Response should not be null");

            Assert.AreEqual(user.ObjectId.ToString(), authResponse.GetUniqueId(), $"Unexpected unique user id in auth response for UPN: {user.Upn}");
        }

        public void AssertAccessTokensAreEqual(IReadOnlyDictionary<string, object> authResponse1,
            IReadOnlyDictionary<string, object> authResponse2)
        {
            AssertAccessTokensAreEqual(authResponse1["access_token"].ToString(), 
                authResponse2["access_token"].ToString());
        }

        public void AssertAccessTokensAreNotEqual(IReadOnlyDictionary<string, object> authResponse1,
            IReadOnlyDictionary<string, object> authResponse2)
        {
            Assert.IsFalse(authResponse1["access_token"].ToString().Equals(authResponse2["access_token"].ToString()), "Access tokens should not be equal.");
        }
        
        /// <summary>
        /// Decode the access tokens and assert that the internal fields are all the same.
        /// WARNING: Deconstructing the token should never be done as part of the test. The only reason
        /// it is done here is to increase the value of a test failure message.
        /// </summary>
        /// <param name="stringToken1"></param>
        /// <param name="stringToken2"></param>
        private void AssertAccessTokensAreEqual(string stringToken1, string stringToken2)
        {
            // If we can't read one of the tokens, fall back to direct string comparison
            if (!_tokenHandler.CanReadToken(stringToken1) || !_tokenHandler.CanReadToken(stringToken2))
            {
                _logger.LogInfo("ResponseValidator: Could not decode one of the tokens (invalid format?), using string comparison.");

                // The below line deliberately doesn't use Assert.AreEqual(), as that would print the tokens
                Assert.IsTrue(stringToken1.Equals(stringToken2), "Expected tokens to be the same.");
            }

            var accessToken1 = _tokenHandler.ReadJwtToken(stringToken1);
            var accessToken2 = _tokenHandler.ReadJwtToken(stringToken2);

            // There's no need to check the headers, but examine the claims
            var claimDictionary1 = accessToken1.Claims.ToDictionary(x => x.Type, x => x.Value);
            var claimDictionary2 = accessToken2.Claims.ToDictionary(x => x.Type, x => x.Value);

            // Assert the most important claims match, these are in the spec RFC 7519 "JSON Web Token (JWT)"
            // (Doing this ahead of the foreach presents more useful information should one of these asserts fail)
            Assert.AreEqual(claimDictionary1["upn"], claimDictionary2["upn"], "Tokens should be for the same user.");
            Assert.AreEqual(claimDictionary1["aud"], claimDictionary2["aud"], "Tokens should be for the same resource.");
            Assert.AreEqual(claimDictionary1["iss"], claimDictionary2["iss"], "Tokens should have the same issuer.");
            Assert.AreEqual(claimDictionary1["iat"], claimDictionary2["iat"], "Tokens should have been issued at the same time.");
            Assert.AreEqual(claimDictionary1["exp"], claimDictionary2["exp"], "Tokens should have the same expiry.");

            // Then go through and assert all claims match
            foreach (var keyValuePair in claimDictionary1)
            {
                string secondValue;
                Assert.IsTrue(claimDictionary2.TryGetValue(keyValuePair.Key, out secondValue),
                    $"Expected second token to contain a claim for {keyValuePair.Key}, but it did not.");
                Assert.AreEqual(keyValuePair.Value, secondValue,
                    $"Expected claims {keyValuePair.Key} to match, but it did not.");
            }
        }

        private string GetProtectedUrl(ResourceType resourceType, string tenantId)
        {
            string protectedUrl = ConfigurationProvider.Instance.GetResource(resourceType)?.ProtectedUrl;

            if (string.IsNullOrWhiteSpace(protectedUrl))
            {
                _logger.LogError($"ResponseValidator: A protected URL for the resourceType ${resourceType} was not configured.");
                Assert.Inconclusive($"A protected URL for the resourceType ${resourceType} was not configured.");
            }

            return string.Format(protectedUrl, tenantId);
        }
    }

}