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
//--

using Microsoft.Identity.Core;
using Microsoft.Identity.Core.Telemetry;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using System.Collections.Generic;
using System.Globalization;
using Test.Microsoft.Identity.Core.Unit;

namespace Test.MSAL.NET.Unit.net45.CoreTests.Telemetry
{   [TestClass]
    public class XmsCliTelemTests
    {

        [TestInitialize]
        public void TestInitialize()
        {
            // Methods in XmsCliTelemTests log errors when parsing response headers;
            CoreLoggerBase.Default = Substitute.For<CoreLoggerBase>();
        }

        [TestMethod]
        [TestCategory("TelemetryInternalAPI")]
        public void XmsClientTelemInfoParseTest_XmsCliTelemInfoCorrectFormat()
        {
            //Arrange
            var requestContext = new RequestContext(CoreLoggerBase.Default);

            //Act - Parse correctly formatted header
            Dictionary<string, string> responseHeaders = new Dictionary<string, string>
            {
                {"x-ms-clitelem", "1,0,0,,"}
            };

            XmsCliTelemInfo xmsCliTeleminfo = XmsCliTelemInfoParser.parseXMsTelemHeader(responseHeaders["x-ms-clitelem"], requestContext);
            
            // Assert
            Assert.AreEqual(xmsCliTeleminfo.Version, "1");
            Assert.AreEqual(xmsCliTeleminfo.ServerErrorCode, "0");
            Assert.AreEqual(xmsCliTeleminfo.ServerSubErrorCode, "0");
            Assert.AreEqual(xmsCliTeleminfo.TokenAge, "");
            Assert.AreEqual(xmsCliTeleminfo.SpeInfo, "");

        }

        [TestMethod]
        [TestCategory("TelemetryInternalAPI")]
        public void XmsClientTelemInfoParseTest_IncorrectFormat()
        {   
            //Arrange
            var requestContext = new RequestContext(CoreLoggerBase.Default);

            //Act - Parse malformed header - 6 values
            Dictionary<string, string> responseHeaders = new Dictionary<string, string>
            {
                {"x-ms-clitelem", "1,2,3,4,5,6"}
            };

            XmsCliTelemInfo xmsCliTeleminfo = XmsCliTelemInfoParser.parseXMsTelemHeader(responseHeaders["x-ms-clitelem"], requestContext);
     
            // Assert
            Assert.IsNull(xmsCliTeleminfo);
            CoreLoggerBase.Default.Received().Warning(Arg.Is(
                            string.Format(CultureInfo.InvariantCulture,
                            TelemetryError.XmsTelemMalformed, responseHeaders["x-ms-clitelem"])));
        }
        
        [TestMethod]
        [TestCategory("TelemetryInternalAPI")]
        public void XmsClientTelemInfoParseTest_IncorrectHeaderVersion()
        {
            //Arrange
            var requestContext = new RequestContext(CoreLoggerBase.Default);

            //Act - Parse wrong version of header - should be "1"
            Dictionary<string, string> responseHeaders = new Dictionary<string, string>
            {
                {"x-ms-clitelem", "3,0,0,,"}
            };

            XmsCliTelemInfo xmsCliTeleminfo = XmsCliTelemInfoParser.parseXMsTelemHeader(responseHeaders["x-ms-clitelem"], requestContext);

            // Assert
            Assert.IsNull(xmsCliTeleminfo);
            CoreLoggerBase.Default.Received().Warning(Arg.Is(
                            string.Format(CultureInfo.InvariantCulture,
                            TelemetryError.XmsUnrecognizedHeaderVersion, "3")));
        }
    }
}
