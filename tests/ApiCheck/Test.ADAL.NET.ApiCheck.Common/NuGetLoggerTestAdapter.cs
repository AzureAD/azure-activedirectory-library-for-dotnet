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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NuGet;

namespace Test.ADAL.ApiCheck
{
    /// <summary>
    /// Adapter between Nuget logger and MSTest
    /// </summary>
    public class NuGetLoggerTestAdapter : ILogger
    {
        private readonly Action<string> logAction;


        public NuGetLoggerTestAdapter(Action<string> logAction)
        {
            this.logAction = logAction;
        }

        public void Log(MessageLevel level, string message, params object[] args)
        {
            string messageToLog = String.Format(
                CultureInfo.InvariantCulture, 
                $"NUGET {level} {message}",
                args);

            logAction(messageToLog);
        }

        public FileConflictResolution ResolveFileConflict(string message)
        {
            // This is the NuGet default 
            return FileConflictResolution.Ignore;
        }
    }
}
