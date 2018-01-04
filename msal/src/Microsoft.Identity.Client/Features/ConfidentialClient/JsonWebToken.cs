﻿//----------------------------------------------------------------------
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

using Microsoft.Identity.Client;
using System;
using System.Runtime.Serialization;
using System.Text;

namespace Microsoft.Identity.Client.Internal.Jwt
{
    internal class JsonWebTokenConstants
    {
        public const uint JwtToAadLifetimeInSeconds = 60*10; // Ten minutes
        public const string HeaderType = "JWT";

        internal class Algorithms
        {
            public const string RsaSha256 = "RS256";
            public const string None = "none";
        }

        internal class ReservedClaims
        {
            public const string Audience = "aud";
            public const string Issuer = "iss";
            public const string Subject = "sub";
            public const string NotBefore = "nbf";
            public const string ExpiresOn = "exp";
            public const string JwtIdentifier = "jti";
        }

        internal class ReservedHeaderParameters
        {
            public const string Algorithm = "alg";
            public const string Type = "typ";
            public const string X509CertificateThumbprint = "kid";
        }
    }

    internal class JsonWebToken
    {
        // (64K) This is an arbitrary large value for the token length. We can adjust it as needed.
        private const int MaxTokenLength = 65536;
        public readonly JWTPayload Payload;

        public JsonWebToken(string clientId, string audience)
        {
            DateTime validFrom = DateTime.UtcNow;

            DateTime validTo = validFrom + TimeSpan.FromSeconds(JsonWebTokenConstants.JwtToAadLifetimeInSeconds);

            Payload = new JWTPayload
            {
                Audience = audience,
                Issuer = clientId,
                ValidFrom = ConvertToTimeT(validFrom),
                ValidTo = ConvertToTimeT(validTo),
                Subject = clientId,
                JwtIdentifier = Guid.NewGuid().ToString()
            };
        }

        public string Sign(ClientAssertionCertificate credential)
        {
            // Base64Url encoded header and claims
            string token = Encode(credential);

            // Length check before sign
            if (MaxTokenLength < token.Length)
            {
                throw new MsalException(MsalError.EncodedTokenTooLong);
            }

            return string.Concat(token, ".", UrlEncodeSegment(credential.Sign(token)));
        }

        private static string EncodeSegment(string segment)
        {
            return UrlEncodeSegment(Encoding.UTF8.GetBytes(segment));
        }

        private static string UrlEncodeSegment(byte[] segment)
        {
            return Base64UrlHelpers.Encode(segment);
        }

        private static string EncodeHeaderToJson(ClientAssertionCertificate credential)
        {
            JWTHeaderWithCertificate header = new JWTHeaderWithCertificate(credential);
            return JsonHelper.SerializeToJson(header);
        }

        internal static long ConvertToTimeT(DateTime time)
        {
            var startTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            TimeSpan diff = time - startTime;
            return (long) (diff.TotalSeconds);
        }

        private string Encode(ClientAssertionCertificate credential)
        {
            // Header segment
            string jsonHeader = EncodeHeaderToJson(credential);

            string encodedHeader = EncodeSegment(jsonHeader);

            // Payload segment
            string jsonPayload = JsonHelper.SerializeToJson(Payload);

            string encodedPayload = EncodeSegment(jsonPayload);

            return string.Concat(encodedHeader, ".", encodedPayload);
        }

        [DataContract]
        internal class JWTHeader
        {
            public JWTHeader(ClientAssertionCertificate credential)
            {
                Credential = credential;
            }

            protected ClientAssertionCertificate Credential { get; }

            [DataMember(Name = JsonWebTokenConstants.ReservedHeaderParameters.Type)]
            public static string Type
            {
                get { return JsonWebTokenConstants.HeaderType; }

                set
                {
                    // This setter is required by DataContractJsonSerializer
                }
            }

            [DataMember(Name = JsonWebTokenConstants.ReservedHeaderParameters.Algorithm)]
            public string Algorithm
            {
                get
                {
                    return Credential == null
                        ? JsonWebTokenConstants.Algorithms.None
                        : JsonWebTokenConstants.Algorithms.RsaSha256;
                }

                set
                {
                    // This setter is required by DataContractJsonSerializer
                }
            }
        }

        [DataContract]
        internal class JWTPayload
        {
            [DataMember(Name = JsonWebTokenConstants.ReservedClaims.Audience)]
            public string Audience { get; set; }

            [DataMember(Name = JsonWebTokenConstants.ReservedClaims.Issuer)]
            public string Issuer { get; set; }

            [DataMember(Name = JsonWebTokenConstants.ReservedClaims.NotBefore)]
            public long ValidFrom { get; set; }

            [DataMember(Name = JsonWebTokenConstants.ReservedClaims.ExpiresOn)]
            public long ValidTo { get; set; }

            [DataMember(Name = JsonWebTokenConstants.ReservedClaims.Subject, IsRequired = false,
                EmitDefaultValue = false)]
            public string Subject { get; set; }

            [DataMember(Name = JsonWebTokenConstants.ReservedClaims.JwtIdentifier, IsRequired = false,
                EmitDefaultValue = false)]
            public string JwtIdentifier { get; set; }
        }

        [DataContract]
        internal sealed class JWTHeaderWithCertificate : JWTHeader
        {
            public JWTHeaderWithCertificate(ClientAssertionCertificate credential)
                : base(credential)
            {
            }

            [DataMember(Name = JsonWebTokenConstants.ReservedHeaderParameters.X509CertificateThumbprint)]
            public string X509CertificateThumbprint
            {
                get
                {
                    // Thumbprint should be url encoded
                    return Credential.Thumbprint;
                }

                set
                {
                    // This setter is required by DataContractJsonSerializer
                }
            }
        }
    }
}