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

using Microsoft.Identity.Core.Exceptions;
using System;

namespace Microsoft.Identity.Client
{
    /// <summary>
    /// Adapter used to convert exceptions to MsalException types
    /// </summary>
    internal static class MsalExceptionAdapter
    {
        /// <summary>
        /// Constructs the appropriate MsalException from the CoreException.
        /// </summary>
        /// <param name="coreException"></param>
        /// <returns></returns>
        public static MsalException FromCoreException(CoreException coreException)
        {
            if (coreException is CoreClientException)
            {
                var coreClientException = coreException as CoreClientException;
                return new MsalClientException(
                    coreClientException.ErrorCode,
                    coreClientException.Message,
                    coreClientException.InnerException)
                {
                    CoreException = coreException
                };
            }

            if (coreException is CoreServiceException)
            {
                var coreServiceException = coreException as CoreServiceException;
                if (!coreServiceException.IsUiRequired)
                {
                    return new MsalServiceException(
                        coreServiceException.ErrorCode,
                        coreServiceException.Message,
                        coreServiceException.InnerException)
                    {
                        CoreException = coreException,
                        Claims = coreServiceException.Claims,
                        ResponseBody = coreServiceException.ResponseBody,
                        StatusCode = coreServiceException.StatusCode
                    };
                }

                return new MsalUiRequiredException(
                    coreServiceException.ErrorCode,
                        coreServiceException.Message,
                        coreServiceException.InnerException)
                {
                    CoreException = coreException,
                    Claims = coreServiceException.Claims,
                    ResponseBody = coreServiceException.ResponseBody,
                    StatusCode = coreServiceException.StatusCode
                };
            }

            throw new InvalidOperationException(
                "Expecting either a CoreClient or CoreService exception. Standalone CoreException should not be used.");
            
        }
    }
}
