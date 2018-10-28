﻿// ------------------------------------------------------------------------------
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
// ------------------------------------------------------------------------------

using System;

namespace Microsoft.Identity.Core.Telemetry
{
    internal class TelemetryHelper : IDisposable
    {
        private readonly EventBase _eventToEnd;
        private readonly string _requestId;
        private readonly string _clientId;
        private readonly bool _shouldFlush;
        private readonly ITelemetry _telemetry;

        public TelemetryHelper(
            ITelemetry telemetry,
            string requestId,
            string clientId,
            EventBase eventToStart,
            EventBase eventToEnd,
            bool shouldFlush)
        {
            _telemetry = telemetry;
            _requestId = requestId;
            _clientId = clientId;
            _eventToEnd = eventToEnd;
            _shouldFlush = shouldFlush;
            _telemetry?.StartEvent(_requestId, eventToStart);
        }

        #region IDisposable Support

        private bool _disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _telemetry?.StopEvent(_requestId, _eventToEnd);
                    if (_shouldFlush)
                    {
                        _telemetry?.Flush(_requestId, _clientId);
                    }
                }

                _disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}