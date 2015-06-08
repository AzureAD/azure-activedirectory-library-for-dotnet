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
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Test.ADAL.Common;

namespace TestApp.PCL
{
    public class TokenBroker
    {
        private readonly AuthenticationContext context;
        private readonly Sts sts;

        public TokenBroker()
        {
            this.sts = new AadSts();
            context = new AuthenticationContext(this.sts.Authority, true);
        }

        public async Task<string> GetTokenInteractiveAsync(IPlatformParameters parameters)
        {
            try
            {
                var result =
                    await
                        context.AcquireTokenAsync(sts.ValidScope, null, sts.ValidClientId,
                            sts.ValidNonExistingRedirectUri, parameters,
                            new UserIdentifier(sts.ValidUserName, UserIdentifierType.OptionalDisplayableId));

                return result.Token;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public async Task<string> GetTokenInteractiveWithMsAppAsync(IPlatformParameters parameters)
        {
            try
            {
                var result =
                    await
                        context.AcquireTokenAsync(sts.ValidScope, null, sts.ValidClientId, null, parameters,
                            new UserIdentifier(sts.ValidUserName, UserIdentifierType.OptionalDisplayableId));

                return result.Token;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public void ClearTokenCache()
        {
            this.context.TokenCache.Clear();
        }
    }
}