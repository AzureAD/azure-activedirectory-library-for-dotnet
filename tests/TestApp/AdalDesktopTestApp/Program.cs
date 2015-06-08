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
using System.Threading.Tasks;

using Microsoft.IdentityModel.Clients.ActiveDirectory;

using TestApp.PCL;

namespace AdalDesktopTestApp
{
    class Program
    {
        private TokenCache cache = null;

        [STAThread]
        static void Main(string[] args)
        {
            Program program = new Program();
            program.AcquireTokenAsync().Wait();
            Console.ReadKey();
        }

        private async Task AcquireTokenAsync()
        {
            Environment.SetEnvironmentVariable("ExtraQueryParameter", "slice=testslice&nux=1&msaproxy=true");
            cache = new TokenCache(this.ReadCacheFile());
            AuthenticationContext context = new AuthenticationContext("https://login.microsoftonline.com/81690286-5054-4f97-b708-541654cd921a/", true, cache);

            IPlatformParameters param = new PlatformParameters(PromptBehavior.Auto, null);
            AuthenticationResult result = await context.AcquireTokenAsync(new[] { "https://outlook.office.com/Mail.Read" }, null,
                "e1eb8a8d-7b0c-4a14-9313-3f2c25c82929", new Uri("urn:ietf:wg:oauth:2.0:oob"), param,
new UserIdentifier("e2e@adalobjc.onmicrosoft.com", UserIdentifierType.RequiredDisplayableId), "slice=testslice&nux=1&msaproxy=true");
            Console.WriteLine(result.Token + "\n");
            this.WriteCacheFile(cache.Serialize());
        }

        private byte[] ReadCacheFile()
        {
            string path = "c:\\git\\cache.txt";
            if (File.Exists(path))
            {
                return File.ReadAllBytes(path);
            }

            return null;
        }
        private void WriteCacheFile(byte[] cacheData)
        {
            string path = "c:\\git\\cache.txt";
            File.WriteAllBytes(path, cacheData);
        }
    }
}
