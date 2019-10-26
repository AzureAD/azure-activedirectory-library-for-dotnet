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

namespace Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.Platform
{
    internal static class AdalErrorIOSEx
    {
        public const string BrokerApplicationRequired = "broker_application_required";
        public const string WritingApplicationTokenToKeychainFailed = "writing_application_token_to_keychain_failed";
        public const string ReadingApplicationTokenFromKeychainFailed = "reading_application_token_from_keychain_failed";
    }

    internal static class AdalErrorMessageIOSEx
    {
        public const string BrokerApplicationRequired = "Broker application must be installed to continue authentication";
        public const string WritingApplicationTokenToKeychainFailed = "This error indidates that the writing of the application token from iOS broker to the" +
            " keychain threw an exception. No SecStatusCode was returned. ";
        public const string ReadingApplicationTokenFromKeychainFailed = "This error indidates that the reading of the application token from they keychain" +
            " threw an exception. No SecStatusCode was returned. ";
    }
}
