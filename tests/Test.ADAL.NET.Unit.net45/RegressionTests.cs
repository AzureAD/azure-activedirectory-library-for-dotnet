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

using Microsoft.Identity.Core;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal;
using Microsoft.IdentityModel.Clients.ActiveDirectory.Internal.ClientCreds;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Threading.Tasks;
using Test.ADAL.NET.Common;
using Test.ADAL.NET.Common.Mocks;

namespace Test.ADAL.NET.Unit.net45
{
    [TestClass]
    public class RegressionTests
    {
        [TestInitialize]
        public void Initialize()
        {
            ModuleInitializer.ForceModuleInitializationTestOnly();
            InstanceDiscovery.InstanceCache.Clear();
        }

        [TestMethod]
        public async Task Double504Async()
        {
            using (var httpManager = new MockHttpManager())
            {
                var serviceBundle = ServiceBundle.CreateWithCustomHttpManager(httpManager);

                // Arrange 
                MockHttpMessageHandler mockHandler1 = MockHelpers.CreateInstanceDiscoveryMockHandler(
                AdalTestConstants.GetDiscoveryEndpoint(AdalTestConstants.DefaultAuthorityCommonTenant));
                MockHttpMessageHandler mockHandler2 = MockHelpers.CreateInstanceDiscoveryMockHandler(
                    AdalTestConstants.GetDiscoveryEndpoint(AdalTestConstants.DefaultAuthorityCommonTenant));

                mockHandler1.ResponseMessage = MockHelpers.CreateResiliencyMessage(HttpStatusCode.GatewayTimeout);
                mockHandler2.ResponseMessage = MockHelpers.CreateResiliencyMessage(HttpStatusCode.GatewayTimeout);

                // Timeouts are retried once, so setup 2 timeouts
                httpManager.AddMockHandler(mockHandler1);
                httpManager.AddMockHandler(mockHandler2);

                AuthenticationContext context = new AuthenticationContext(
                    serviceBundle,
                    AdalTestConstants.DefaultAuthorityCommonTenant,
                    AuthorityValidationType.True,
                    null);

                try
                {
                    // Act
                    await context.AcquireTokenForClientCommonAsync(
                        AdalTestConstants.DefaultResource, new ClientKey(AdalTestConstants.DefaultClientId))
                        .ConfigureAwait(false);
                }
                catch (AdalException ex)
                {
                    Assert.AreEqual(AdalError.AuthorityValidationFailed, ex.ErrorCode);
                    AdalServiceException inner = ex.InnerException as AdalServiceException;
                    Assert.AreEqual((int)HttpStatusCode.GatewayTimeout, inner.StatusCode);
                    return;
                }

                Assert.Fail("Expecting a timeout ADAL Exception to have been thrown");
            }
        }

        [TestMethod]
        [WorkItem(1489)] // https://github.com/AzureAD/azure-activedirectory-library-for-dotnet/issues/1489
        public async Task DoubleTimeoutDoesNotResultInANullRef_Async()
        {
            using (var httpManager = new MockHttpManager())
            {
                var serviceBundle = ServiceBundle.CreateWithCustomHttpManager(httpManager);

                // Arrange 
                MockHttpMessageHandler mockHandler1 = MockHelpers.CreateInstanceDiscoveryMockHandler(
                AdalTestConstants.GetDiscoveryEndpoint(AdalTestConstants.DefaultAuthorityCommonTenant));
                MockHttpMessageHandler mockHandler2 = MockHelpers.CreateInstanceDiscoveryMockHandler(
                    AdalTestConstants.GetDiscoveryEndpoint(AdalTestConstants.DefaultAuthorityCommonTenant));

                mockHandler1.ExceptionToThrow = new TaskCanceledException("First Timeout");
                mockHandler2.ExceptionToThrow = new TaskCanceledException("Second Timeout");

                // Timeouts are retried once, so setup 2 timeouts
                httpManager.AddMockHandler(mockHandler1);
                httpManager.AddMockHandler(mockHandler2);

                AuthenticationContext context = new AuthenticationContext(
                    serviceBundle,
                    AdalTestConstants.DefaultAuthorityCommonTenant,
                    AuthorityValidationType.True,
                    null);

                try
                {
                    // Act
                    await context.AcquireTokenForClientCommonAsync(
                        AdalTestConstants.DefaultResource, new ClientKey(AdalTestConstants.DefaultClientId))
                        .ConfigureAwait(false);
                }
                catch (AdalException ex)
                {
                    // Assert

                    // Instance Discovery will wrap all exceptions, so the task cancelled is burried 2 times deep
                    Assert.AreSame(mockHandler2.ExceptionToThrow.Message, ex.InnerException.InnerException.InnerException.Message);
                    return;
                }

                Assert.Fail("Expecting a timeout ADAL Service Exception to have been thrown");
            }
        }
    }
}
