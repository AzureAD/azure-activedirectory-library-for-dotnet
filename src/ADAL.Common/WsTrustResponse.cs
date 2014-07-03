﻿//----------------------------------------------------------------------
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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Microsoft.IdentityModel.Clients.ActiveDirectory
{
    class WsTrustResponse
    {
        public const string Saml1Assertion = "urn:oasis:names:tc:SAML:1.0:assertion";

        public string Token { get; private set; }

        public string TokenType { get; private set; }

        public static WsTrustResponse CreateFromResponse(Stream responseStream)
        {
            XDocument responseDocument = ReadDocumentFromResponse(responseStream);
            return CreateFromResponseDocument(responseDocument);
        }

        public static string ReadErrorResponse(XDocument responseDocument)
        {
            string errorMessage = null;

            try
            {
                XElement body = responseDocument.Descendants(XmlNamespace.SoapEnvelope + "Body").FirstOrDefault();

                if (body != null)
                {
                    XElement fault = body.Elements(XmlNamespace.SoapEnvelope + "Fault").FirstOrDefault();
                    if (fault != null)
                    {
                        XElement reason = fault.Elements(XmlNamespace.SoapEnvelope + "Reason").FirstOrDefault();
                        if (reason != null)
                        {
                            XElement text = reason.Elements(XmlNamespace.SoapEnvelope + "Text").FirstOrDefault();
                            if (text != null)
                            {
                                using (var reader = text.CreateReader())
                                {
                                    reader.MoveToContent();
                                    errorMessage = reader.ReadInnerXml();
                                }
                            }
                        }
                    }
                }
            }
            catch (XmlException ex)
            {
                throw new AdalException(AdalError.ParsingWsTrustResponseFailed, ex);
            }

            return errorMessage;
        }

        internal static XDocument ReadDocumentFromResponse(Stream responseStream)
        {
            try
            {
                return XDocument.Load(responseStream, LoadOptions.None);
            }
            catch (XmlException ex)
            {
                throw new AdalException(AdalError.ParsingWsTrustResponseFailed, ex);
            }
        }

        internal static WsTrustResponse CreateFromResponseDocument(XDocument responseDocument)
        {
            Dictionary<string, string> tokenResponseDictionary = new Dictionary<string, string>();

            try
            {
                XElement requestSecurityTokenResponseCollection =
                    responseDocument.Descendants(XmlNamespace.Trust + "RequestSecurityTokenResponseCollection").FirstOrDefault();

                if (requestSecurityTokenResponseCollection != null)
                {
                    IEnumerable<XElement> tokenResponses = responseDocument.Descendants(XmlNamespace.Trust + "RequestSecurityTokenResponse");
                    foreach (var tokenResponse in tokenResponses)
                    {
                        XElement tokenTypeElement = tokenResponse.Elements(XmlNamespace.Trust + "TokenType").FirstOrDefault();
                        if (tokenTypeElement == null)
                        {
                            continue;
                        }

                        XElement requestedSecurityToken = tokenResponse.Elements(XmlNamespace.Trust + "RequestedSecurityToken").FirstOrDefault();
                        if (requestedSecurityToken == null)
                        {
                            continue;
                        }

                        // TODO #123622: We need to disable formatting due to a potential service bug. Remove the ToString argument when problem is fixed.
                        tokenResponseDictionary.Add(tokenTypeElement.Value, requestedSecurityToken.FirstNode.ToString(SaveOptions.DisableFormatting));
                    }
                }
            }
            catch (XmlException ex)
            {
                throw new AdalException(AdalError.ParsingWsTrustResponseFailed, ex);
            }

            if (tokenResponseDictionary.Count == 0)
            {
                throw new AdalException(AdalError.ParsingWsTrustResponseFailed);
            }

            string tokenType = tokenResponseDictionary.ContainsKey(Saml1Assertion) ? Saml1Assertion : tokenResponseDictionary.Keys.First();

            WsTrustResponse wsTrustResponse = new WsTrustResponse
                                              {
                                                  TokenType = tokenType,
                                                  Token = tokenResponseDictionary[tokenType]
                                              };

            return wsTrustResponse;
        }
    }
}
