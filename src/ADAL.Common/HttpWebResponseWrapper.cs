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

using System;
using System.IO;
using System.Net;

namespace Microsoft.IdentityModel.Clients.ActiveDirectory
{
    internal class HttpWebResponseWrapper : IHttpWebResponse
    {
        private WebResponse _response;

        public HttpWebResponseWrapper(WebResponse response)
        {
            this._response = response;
        }

        public HttpStatusCode StatusCode
        {
            get
            {
                var httpWebResponse = this._response as HttpWebResponse;
                return (httpWebResponse != null) ? httpWebResponse.StatusCode : HttpStatusCode.NotImplemented;
            }
        }

        public WebHeaderCollection Headers
        {
            get
            {
                return this._response.Headers;
            }
        }

        public Stream GetResponseStream()
        {
            Stream stream = new MemoryStream();
            BinaryReader binReader = new BinaryReader(_response.GetResponseStream());
            const int bufferSize = 4096;
            long maxResponseSizeInBytes = 1048576;
            
            byte[] buffer = new byte[bufferSize];
            int count;
            while ((count = binReader.Read(buffer, 0, buffer.Length)) != 0)
            {
                stream.Write(buffer, 0, count);
                maxResponseSizeInBytes -= count;

                if (maxResponseSizeInBytes < 0)
                {
                    throw new AdalException(AdalError.HttpResponseContentLengthOverLimit);
                }
            }

            stream.Position = 0;
            return stream;
        }

        public void Close()
        {
            PlatformSpecificHelper.CloseHttpWebResponse(this._response);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_response != null)
                {
                    ((IDisposable)_response).Dispose();
                    _response = null;
                }
            }
        }
    }
}