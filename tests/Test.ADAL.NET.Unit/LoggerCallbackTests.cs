//------------------------------------------------------------------------------
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
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.Platform;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Test.ADAL.NET.Unit
{
    class TestCallback : IAdalLogCallback
    {
        public int ErrorLogCount { get; private set; }
        public int InfoLogCount { get; private set; }
        public int WarningLogCount { get; private set; }
        public int VerboseLogCount { get; private set; }

        public int PiiErrorLogCount { get; private set; }
        public int PiiInfoLogCount { get; private set; }
        public int PiiWarningLogCount { get; private set; }
        public int PiiVerboseLogCount { get; private set; }

        public void Log(LogLevel level, string message, bool containsPii)
        {
            switch (level)
            {
                case LogLevel.Error:
                    if (containsPii)
                    {
                        PiiErrorLogCount += 1;
                    }
                    else
                    {
                        ErrorLogCount += 1;
                    }
                    break;
                case LogLevel.Warning:
                    if (containsPii)
                    {
                        PiiWarningLogCount += 1;
                    }
                    else
                    {
                        WarningLogCount += 1;
                    }
                    break;
                case LogLevel.Information:
                    if (containsPii)
                    {
                        PiiInfoLogCount += 1;
                    }
                    else
                    {
                        InfoLogCount += 1;
                    }
                    break;
                case LogLevel.Verbose:
                    if (containsPii)
                    {
                        PiiVerboseLogCount += 1;
                    }
                    else
                    {
                        VerboseLogCount += 1;
                    }
                    break;
            }
        }
    }

    [TestClass]
    public class LoggerCallbackTests
    {
        private const string Message = "test message";

        [TestMethod]
        [TestCategory("LoggerCallbackTests")]
        public void LogTest()
        {
            var logger = new Logger();
            var state = new CallState(Guid.NewGuid());
            var callback = new TestCallback();
            LoggerCallbackHandler.Callback = callback;
            LoggerCallbackHandler.PiiLoggingEnabled = true;

            logger.Error(state, new Exception(Message));
            Assert.AreEqual(1, callback.ErrorLogCount);
            Assert.AreEqual(0, callback.WarningLogCount);
            Assert.AreEqual(0, callback.InfoLogCount);
            Assert.AreEqual(0, callback.VerboseLogCount);

            logger.Information(state, Message);
            Assert.AreEqual(1, callback.ErrorLogCount);
            Assert.AreEqual(0, callback.WarningLogCount);
            Assert.AreEqual(1, callback.InfoLogCount);
            Assert.AreEqual(0, callback.VerboseLogCount);

            logger.Verbose(state, Message);
            Assert.AreEqual(1, callback.ErrorLogCount);
            Assert.AreEqual(0, callback.WarningLogCount);
            Assert.AreEqual(1, callback.InfoLogCount);
            Assert.AreEqual(1, callback.VerboseLogCount);

            logger.Warning(state, Message);
            Assert.AreEqual(1, callback.ErrorLogCount);
            Assert.AreEqual(1, callback.WarningLogCount);
            Assert.AreEqual(1, callback.InfoLogCount);
            Assert.AreEqual(1, callback.VerboseLogCount);

            // make sure no calls to Log with containsPii = true
            Assert.AreEqual(0, callback.PiiErrorLogCount);
            Assert.AreEqual(0, callback.PiiWarningLogCount);
            Assert.AreEqual(0, callback.PiiInfoLogCount);
            Assert.AreEqual(0, callback.PiiVerboseLogCount);
        }

        [TestMethod]
        [TestCategory("LoggerCallbackTests")]
        public void PiiLogTest()
        {
            var logger = new Logger();
            var state = new CallState(Guid.NewGuid());
            var callback = new TestCallback();
            LoggerCallbackHandler.Callback = callback;
            LoggerCallbackHandler.PiiLoggingEnabled = true;

            logger.ErrorPii(state, new Exception(Message));
            Assert.AreEqual(1, callback.PiiErrorLogCount);
            Assert.AreEqual(0, callback.PiiWarningLogCount);
            Assert.AreEqual(0, callback.PiiInfoLogCount);
            Assert.AreEqual(0, callback.PiiVerboseLogCount);

            logger.InformationPii(state, Message);
            Assert.AreEqual(1, callback.PiiErrorLogCount);
            Assert.AreEqual(0, callback.PiiWarningLogCount);
            Assert.AreEqual(1, callback.PiiInfoLogCount);
            Assert.AreEqual(0, callback.PiiVerboseLogCount);

            logger.VerbosePii(state, Message);
            Assert.AreEqual(1, callback.PiiErrorLogCount);
            Assert.AreEqual(0, callback.PiiWarningLogCount);
            Assert.AreEqual(1, callback.PiiInfoLogCount);
            Assert.AreEqual(1, callback.PiiVerboseLogCount);

            logger.WarningPii(state, Message);
            Assert.AreEqual(1, callback.PiiErrorLogCount);
            Assert.AreEqual(1, callback.PiiWarningLogCount);
            Assert.AreEqual(1, callback.PiiInfoLogCount);
            Assert.AreEqual(1, callback.PiiVerboseLogCount);

            // make sure no calls to Log with containsPii = false
            Assert.AreEqual(0, callback.ErrorLogCount);
            Assert.AreEqual(0, callback.WarningLogCount);
            Assert.AreEqual(0, callback.InfoLogCount);
            Assert.AreEqual(0, callback.VerboseLogCount);
        }

        [TestMethod]
        [TestCategory("LoggerCallbackTests")]
        public void NullCallbackTest()
        {
            var logger = new Logger();
            var state = new CallState(Guid.NewGuid());

            logger.Error(state, new Exception(Message));
            logger.Information(state, Message);
            logger.Verbose(state, Message);
            logger.Warning(state, Message);
        }

        [TestMethod]
        [TestCategory("LoggerCallbackTests")]
        public void DefaultLog_UseDefaultLoggingIsTrue_Logged()
        {
            var logger = Substitute.ForPartsOf<Logger>();

            var defaultLogCounter = 0;
            logger.When(x => x.DefaultLog(Arg.Any<LogLevel>(), Arg.Any<string>())).Do(x => defaultLogCounter++);

            var state = new CallState(Guid.NewGuid());
            var callback = new TestCallback();

            LoggerCallbackHandler.Callback = callback;
            LoggerCallbackHandler.PiiLoggingEnabled = true;
            LoggerCallbackHandler.UseDefaultLogging = true;

            logger.Verbose(state, Message);
            Assert.AreEqual(1, defaultLogCounter);

            logger.Information(state, Message);
            Assert.AreEqual(2, defaultLogCounter);

            logger.Warning(state, Message);
            Assert.AreEqual(3, defaultLogCounter);

            logger.Error(state, new Exception(Message));
            Assert.AreEqual(4, defaultLogCounter);
        }

        [TestMethod]
        [TestCategory("LoggerCallbackTests")]
        public void DefaultLog_UseDefaultLoggingIsFalse_NotLogged()
        {
            var logger = Substitute.ForPartsOf<Logger>();

            var defaultLogCounter = 0;
            logger.When(x => x.DefaultLog(Arg.Any<LogLevel>(), Arg.Any<string>())).Do(x => defaultLogCounter++);

            var state = new CallState(Guid.NewGuid());
            var callback = new TestCallback();

            LoggerCallbackHandler.Callback = callback;
            LoggerCallbackHandler.PiiLoggingEnabled = true;
            LoggerCallbackHandler.UseDefaultLogging = false;

            logger.Verbose(state, Message);
            Assert.AreEqual(0, defaultLogCounter);

            logger.Information(state, Message);
            Assert.AreEqual(0, defaultLogCounter);

            logger.Warning(state, Message);
            Assert.AreEqual(0, defaultLogCounter);

            logger.Error(state, new Exception(Message));
            Assert.AreEqual(0, defaultLogCounter);
        }

        [TestMethod]
        [TestCategory("LoggerCallbackTests")]
        public void DefaultLog_UseDefaultLoggingIsTrueContainsPii_PiiNotLogged()
        {
            var logger = Substitute.ForPartsOf<Logger>();

            var piiCounter = 0;
            logger.When(x => x.DefaultLog(Arg.Any<LogLevel>(), Arg.Any<string>())).Do(x => piiCounter++);

            var state = new CallState(Guid.NewGuid());
            var callback = new TestCallback();

            LoggerCallbackHandler.Callback = callback;
            LoggerCallbackHandler.PiiLoggingEnabled = true;
            LoggerCallbackHandler.UseDefaultLogging = true;

            logger.VerbosePii(state, Message);
            logger.InformationPii(state, Message);
            logger.WarningPii(state, Message);
            logger.ErrorPii(state, new Exception(Message));

            Assert.AreEqual(0, piiCounter);
        }
    }
}
