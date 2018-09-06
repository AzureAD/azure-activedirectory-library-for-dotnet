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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Identity.Core;

namespace Test.Microsoft.Identity.Core.Unit
{
    internal class TestPlatformInformation : CorePlatformInformationBase
    {
        public const string TestProductName = "test";
        public const string TestCPU = "test cpu";
        public const string TestOS = "test os";
        public const string TestDevice = "test device";
        public const string TestAssemblyFileVersion = "1.0.0.0";

        static TestPlatformInformation()
        {
            Instance = new TestPlatformInformation();
        }

        public override string GetProductName()
        {
            return TestProductName;
        }

        public override string GetEnvironmentVariable(string variable)
        {
            return null;
        }

        public override string GetProcessorArchitecture()
        {
            return TestCPU;
        }

        public override string GetOperatingSystem()
        {
            return TestOS;
        }

        public override string GetDeviceModel()
        {
            return TestDevice;
        }

        public override string GetAssemblyFileVersionAttribute()
        {
            return TestAssemblyFileVersion;
        }

        public override Task<bool> IsUserLocalAsync(RequestContext requestContext)
        {
            throw new NotImplementedException();
        }
    }
}
