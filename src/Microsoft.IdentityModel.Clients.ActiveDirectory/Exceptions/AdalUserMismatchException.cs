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
using System.Globalization;
using Microsoft.Identity.Core.Helpers;
using Microsoft.Identity.Json.Linq;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal;

namespace Microsoft.IdentityModel.Clients.ActiveDirectory
{
    /// <summary>
    /// The exception type thrown when user returned by service does not match user in the request.
    /// </summary>
    public class AdalUserMismatchException : AdalException
    {
        /// <summary>
        ///  Initializes a new instance of the exception class.
        /// </summary>
        public AdalUserMismatchException(string requestedUser, string returnedUser)
            : base(AdalError.UserMismatch, 
                   string.Format(CultureInfo.CurrentCulture, AdalErrorMessage.UserMismatch, returnedUser, requestedUser))
        {
            RequestedUser = requestedUser;
            ReturnedUser = returnedUser;
        }

        /// <summary>
        /// Gets the user requested from service.
        /// </summary>
        public string RequestedUser { get; private set; }

        /// <summary>
        /// Gets the user returned by service.
        /// </summary>
        public string ReturnedUser { get; private set; }

        /// <summary>
        /// Creates and returns a string representation of the current exception.
        /// </summary>
        /// <returns>A string representation of the current exception.</returns>
        public override string ToString()
        {
            return base.ToString() + string.Format(CultureInfo.CurrentCulture, "\n\tRequestedUser: {0}\n\tReturnedUser: {1}", this.RequestedUser, this.ReturnedUser);
        }

        private const string RequestedUserKey = "requested_user";
        private const string ReturnedUserKey = "returned_user";

        internal override void PopulateJson(JObject jobj)
        {
            base.PopulateJson(jobj);

            jobj[RequestedUserKey] = RequestedUser;
            jobj[ReturnedUserKey] = ReturnedUser;
        }

        internal override void PopulateObjectFromJson(JObject jobj)
        {
            base.PopulateObjectFromJson(jobj);

            RequestedUser = JsonUtils.GetExistingOrEmptyString(jobj, RequestedUserKey);
            ReturnedUser = JsonUtils.GetExistingOrEmptyString(jobj, ReturnedUserKey);
        }

        internal static AdalException FromJsonStringExplicit(JObject jobj)
        {
            string requestedUser = JsonUtils.GetExistingOrEmptyString(jobj, RequestedUserKey);
            string returnedUser = JsonUtils.GetExistingOrEmptyString(jobj, ReturnedUserKey);

            AdalException ex = new AdalUserMismatchException(requestedUser, returnedUser);
            ex.PopulateObjectFromJson(jobj);
            return ex;
        }
    }
}
