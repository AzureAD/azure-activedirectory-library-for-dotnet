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
//------------------------------------------------------------------------------

using Test.Microsoft.Identity.LabInfrastructure;
using NUnit.Framework;
using Test.Microsoft.Identity.Core.UIAutomation;
using Xamarin.UITest;
using Xamarin.UITest.Queries;

//NOTICE! Inorder to run UI automation tests for xamarin locally, you may need to upgrade nunit to 3.0 and above for this project and the core ui Automation project.
//It is set to 2.6.4 because that is the maximum version that appcenter can support.
//There is an error in visual studio that can prevent the NUnit test framework from loading the test dll properly.
//Remember to return the version back to 2.6.4 before commiting to prevent appcenter from failing

namespace Test.ADAL.UIAutomation
{
    /// <summary>
    /// Configures environment for core/android tests to run
    /// </summary>
    [TestFixture(Platform.Android)]
    class XamarinDroidADALTests
    {
        IApp app;
        Platform platform;
        ITestController xamarinController = new XamarinUITestController();

        public XamarinDroidADALTests(Platform platform)
        {
            this.platform = platform;
        }

        /// <summary>
        /// Initializes app and test controller before each test
        /// </summary>
        [SetUp]
        public void InitializeTest()
        {
            app = AppFactory.StartApp(platform, "com.Microsoft.XFormsDroid.ADAL");
            xamarinController.Application = app;
        }

        /// <summary>
        /// Runs through the standard acquire token flow
        /// </summary>
        [Test]
        public void AcquireTokenInteractiveTest()
        {
            ADALMobileTestHelper.AcquireTokenInteractiveTestHelper(xamarinController, LabUserHelper.DefaultUserQuery);
        }

        /// <summary>
        /// Runs through the standard acquire token silent flow 
        /// </summary>
        [Test]
        public void AcquireTokenSilentTest()
        {
            ADALMobileTestHelper.AcquireTokenSilentTestHelper(xamarinController, LabUserHelper.DefaultUserQuery);
        }

        /// <summary>
        /// Runs through the standard acquire token flow with a ADFS V4 account
        /// </summary>
        [Test]
        public void AcquireTokenADFSv4FederatedInteractiveTest()
        {
            var user = LabUserHelper.DefaultUserQuery;
            user.FederationProvider = FederationProvider.AdfsV4;
            user.IsFederatedUser = true;

            ADALMobileTestHelper.AcquireTokenInteractiveTestHelper(xamarinController, user);
        }

        /// <summary>
        /// Runs through the standard acquire token flow
        /// </summary>
        [Test]
        public void AcquireTokenInteractiveADFSv4NonFederatedTest()
        {
            var user = LabUserHelper.DefaultUserQuery;
            user.FederationProvider = FederationProvider.AdfsV4;
            user.IsFederatedUser = false;

            ADALMobileTestHelper.AcquireTokenInteractiveTestHelper(xamarinController, user);
        }

        /// <summary>
        /// Runs through the standard acquire token flow
        /// </summary>
        [Test]
        public void AcquireTokenADFSv3FederatedInteractiveTest()
        {
            var user = LabUserHelper.DefaultUserQuery;
            user.FederationProvider = FederationProvider.AdfsV3;
            user.IsFederatedUser = true;

            ADALMobileTestHelper.AcquireTokenInteractiveTestHelper(xamarinController, user);
        }

        /// <summary>
        /// Runs through the standard acquire token flow
        /// </summary>
        [Test]
        public void AcquireTokenInteractiveADFSv3NonFederatedTest()
        {
            var user = LabUserHelper.DefaultUserQuery;
            user.FederationProvider = FederationProvider.AdfsV3;
            user.IsFederatedUser = false;

            ADALMobileTestHelper.AcquireTokenInteractiveTestHelper(xamarinController, user);
        }
    }
}
