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

namespace DesktopApp
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            while(true)
            {
                // display menu
                // clear display
                Console.Clear();

                Console.WriteLine("\n1. Acquire Token\n2. Acquire Token Conditional Access Policy\n0. Exit App");
                Console.WriteLine("\nEnter your selection: ");

                int selection;

                int.TryParse(Console.ReadLine(), out selection);

                switch(selection)
                {
                    case 1: // acquire token
                        try
                        {
                            AcquireTokenAsync().Wait();
                        }
                        catch(AggregateException ae)
                        {
                            Console.WriteLine(ae.InnerException.Message);
                            Console.WriteLine(ae.InnerException.StackTrace);
                        }
                        break;
                    case 2: // acquire token with claims
                        try
                        {
                            AcquireTokenWithClaimsAsync().Wait();
                        }
                        catch (AggregateException ae)
                        {
                            Console.WriteLine(ae.InnerException.Message);
                            Console.WriteLine(ae.InnerException.StackTrace);
                        }
                        break;
                    case 0:
                        return;
                    default:
                        break;
                }

                Console.WriteLine("\n\nHit 'ENTER' to continue...");
                Console.ReadLine();
            }
        }

        private static async Task AcquireTokenAsync()
        {
            AuthenticationContext context = new AuthenticationContext("https://login.microsoftonline.de/common", true);
            var result = await context.AcquireTokenAsync("https://graph.cloudapi.de", "1950a258-227b-4e31-a9cf-717495945fc2", new UserCredential("Adi.Lette@awesomemate.eu", "P@ssw0rd"));
                
            string token = result.AccessToken;
            Console.WriteLine(token + "\n");
        }

        private static async Task AcquireTokenWithClaimsAsync()
        {
            string claims = "{\"access_token\":{\"polids\":{\"essential\":true,\"values\":[\"5ce770ea-8690-4747-aa73-c5b3cd509cd4\"]}}}";

            AuthenticationContext context = new AuthenticationContext("https://login.microsoftonline.com/common", true);
            var result = await context.AcquireTokenAsync("https://graph.windows.net", "<CLIENT_ID>",
                new Uri("<REDIRECT_URI>"), PromptBehavior.Auto, new UserIdentifier("<USER>", UserIdentifierType.OptionalDisplayableId), null, claims).ConfigureAwait(false);

            string token = result.AccessToken;
            Console.WriteLine(token + "\n");
        }
    }
}
