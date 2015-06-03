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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.IdentityModel.Clients.ActiveDirectory;

using TestApp.PCL;

namespace AdalDesktopTestApp
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            AcquireTokenAsync().Wait();
            Console.ReadKey();
        }

        private static async Task AcquireTokenAsync()
        {
            Environment.SetEnvironmentVariable("ExtraQueryParameter", "slice=testslice&nux=1&msaproxy=true");
            AuthenticationContext context = new AuthenticationContext("https://login.microsoftonline.com/common/", true);
            IPlatformParameters param = new PlatformParameters(PromptBehavior.Auto, null);
            AuthenticationResult result = await context.AcquireTokenAsync(new[] {"https://outlook.office.com/Mail.Read"}, null,
                "e1eb8a8d-7b0c-4a14-9313-3f2c25c82929", new Uri("urn:ietf:wg:oauth:2.0:oob"), param,
                UserIdentifier.AnyUser, "slice=testslice&nux=1&msaproxy=true");
            Console.WriteLine(result.Token + "\n");
        }
    }
}
