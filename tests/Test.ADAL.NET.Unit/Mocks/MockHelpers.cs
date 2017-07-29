//----------------------------------------------------------------------
// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
// Apache License 2.0
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//----------------------------------------------------------------------

using System.Globalization;
using System.IO;
using System.Net;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Test.ADAL.Common;

namespace Test.ADAL.NET.Unit.Mocks
{
    internal static class MockHelpers
    {
        public static Stream GenerateStreamFromString(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static IHttpWebResponse CreateInvalidRequestTokenResponseMessage()
        {
            return
                CreateFailureResponseMessage(
                    "{\"error\":\"invalid_request\",\"error_description\":\"AADSTS70002: Some error message. Trace ID: f7ec686c-9196-4220-a754-cd9197de44e9 Correlation ID: 04bb0cae-580b-49ac-9a10-b6c3316b1eaa Timestamp: 2015-09-16 07:24:55Z\",\"error_codes\":[70002,70008],\"timestamp\":\"2015-09-16 07:24:55Z\",\"trace_id\":\"f7ec686c-9196-4220-a754-cd9197de44e9\",\"correlation_id\":\"04bb0cae-580b-49ac-9a10-b6c3316b1eaa\"}");
        }

        public static IHttpWebResponse CreateInvalidGrantTokenResponseMessage()
        {
            return
                CreateFailureResponseMessage(
                    "{\"error\":\"invalid_grant\",\"error_description\":\"AADSTS70002: Error validating credentials.AADSTS70008: The provided access grant is expired or revoked.Trace ID: f7ec686c-9196-4220-a754-cd9197de44e9Correlation ID: 04bb0cae-580b-49ac-9a10-b6c3316b1eaaTimestamp: 2015-09-16 07:24:55Z\",\"error_codes\":[70002,70008],\"timestamp\":\"2015-09-16 07:24:55Z\",\"trace_id\":\"f7ec686c-9196-4220-a754-cd9197de44e9\",\"correlation_id\":\"04bb0cae-580b-49ac-9a10-b6c3316b1eaa\"}");
        }

        public static IHttpWebResponse CreateFailureResponseMessage(string message)
        {
            IHttpWebResponse responseMessage = new MockHttpWebResponse()
            {
                Stream =
                    GenerateStreamFromString(message),
                StatusCode = HttpStatusCode.BadRequest
            };

            return responseMessage;
        }


        public static IHttpWebResponse CreateSuccessTokenResponseMessage()
        {
            return CreateSuccessTokenResponseMessage("{\"token_type\":\"Bearer\",\"expires_in\":\"3600\"," +
                                                     "\"resource\":\"resource1\",\"access_token\":\"some-access-token\"," +
                                                     "\"refresh_token\":\"something-encrypted\",\"id_token\":\"" +
                                                     CreateIdToken(TestConstants.DefaultUniqueId,
                                                         TestConstants.DefaultDisplayableId) +
                                                     "\"}");
        }

        public static IHttpWebResponse CreateSuccessTokenResponseMessage(string message)
        {
            IHttpWebResponse responseMessage = new MockHttpWebResponse()
            {
                Stream =
                    GenerateStreamFromString(message),
                StatusCode = HttpStatusCode.OK
            };

            return responseMessage;
        }

        public static IHttpWebResponse CreateSuccessfulClientCredentialTokenResponseMessage()
        {
            return
                CreateSuccessTokenResponseMessage(
                    "{\"token_type\":\"Bearer\",\"expires_in\":\"3599\",\"access_token\":\"header.payload.signature\"}");
        }

        public static IHttpWebResponse CreateSuccessTokenResponseMessage(string uniqueId, string displayableId, string resource)
        {
            string idToken = CreateIdToken(uniqueId, displayableId);
            return
                CreateSuccessTokenResponseMessage("{\"token_type\":\"Bearer\",\"expires_in\":\"3599\",\"resource\":\"" +
                                                  resource +
                                                  "\",\"access_token\":\"some-access-token\",\"refresh_token\"" +
                                                  ":\"OAAsomethingencryptedQwgAA\",\"id_token\":\"" +
                                                  idToken +
                                                  "\"}");
        }

        public static IHttpWebResponse CreateClaimsChallengeAndInteractionRequiredResponseMessage()
        {
            string claims = "{\\\"access_token\\\":{\\\"polids\\\":{\\\"essential\\\":true,\\\"values\\\":[\\\"5ce770ea-8690-4747-aa73-c5b3cd509cd4\\\"]}}}";

            string responseContent = "{\"error\":\"interaction_required\",\"claims\":\"" + claims + "\"}";

            IHttpWebResponse responseMessage = new MockHttpWebResponse()
            {
                Stream =
                GenerateStreamFromString(responseContent),
                StatusCode = HttpStatusCode.BadRequest
            };

            return responseMessage;
        }

        private static string CreateIdToken(string uniqueId, string displayableId)
        {
            string header = "{alg: \"none\"," +
                             "typ:\"JWT\"" +
                             "}";
            string payload = "{\"aud\": \"e854a4a7-6c34-449c-b237-fc7a28093d84\"," +
                        "\"iss\": \"https://login.microsoftonline.com/6c3d51dd-f0e5-4959-b4ea-a80c4e36fe5e/\"," +
                        "\"iat\": 1455833828," +
                        "\"nbf\": 1455833828," +
                        "\"exp\": 1455837728," +
                        "\"ipaddr\": \"131.107.159.117\"," +
                        "\"name\": \"Mario Rossi\"," +
                        "\"oid\": \"" + uniqueId + "\"," +
                        "\"upn\": \"" + displayableId + "\"," +
                        "\"sub\": \"werwerewrewrew-Qd80ehIEdFus\"," +
                        "\"tid\": \"some-tenant-id\"," +
                        "\"ver\": \"2.0\"}";

            return string.Format(CultureInfo.InvariantCulture, "{0}.{1}.signature", Base64UrlEncoder.Encode(header), Base64UrlEncoder.Encode(payload));
        }
    }
}
