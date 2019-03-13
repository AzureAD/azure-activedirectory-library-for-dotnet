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
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace AdalDesktopTestApp
{
    class Program
    {
        private static AppLogger AppLogger { get; } = new AppLogger();

        private const string ClientId = "1950a258-227b-4e31-a9cf-717495945fc2";
        private const string RedirectUri = "urn:ietf:wg:oauth:2.0:oob"; //"https://ClientReplyUrl";
        private const string User = ""; // can also be empty string for testing IWA and U/P
        private const string Resource = "https://graph.windows.net";

        [STAThread]
        static void Main(string[] args)
        {
            LoggerCallbackHandler.LogCallback = AppLogger.Log;
            var context = new AuthenticationContext("https://login.windows.net/common", true, new FileCache());

#pragma warning disable CS0162 // Unreachable code detected
            RunAppAsync(context).Wait();
#pragma warning restore CS0162 // Unreachable code detected
        }

        private static async Task RunAppAsync(AuthenticationContext context)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine(string.Format(CultureInfo.CurrentCulture, "TokenCache contains {0} token(s)", context.TokenCache.Count));
                foreach (var item in context.TokenCache.ReadItems())
                {
                    Console.WriteLine("  Cache item available for: " + item.DisplayableId + "\n");
                }

                // display menu
                Console.WriteLine(@"
                        1. Clear the cache
                        2. Acquire Token by Integrated Windows Auth
                        3. Acquire Token Interactively
                        4. Acquire Token Interactively with PromptBehavior=Never
                        5. Acquire Token with Username and Password
                        6. Acquire Token Silently
                        7. Acquire Token by Device Code
                        8. Acquire Token Silent + Interactive with custom Http Client

                        0. Exit App
                    Enter your Selection: ");

                int.TryParse(Console.ReadLine(), out var selection);

                try
                {
                    Task<AuthenticationResult> authTask = null;
                    LoggerCallbackHandler.PiiLoggingEnabled = true;
                    switch (selection)
                    {
                        case 1:
                            // clear cache
                            context.TokenCache.Clear();
                            break;
                        case 2: // acquire token IWA
                            authTask = context.AcquireTokenAsync(Resource, ClientId, new UserCredential(User));
                            await FetchTokenAsync(authTask).ConfigureAwait(false);
                            break;
                        case 3: // acquire token interactive
                            authTask = context.AcquireTokenAsync(Resource, ClientId, new Uri(RedirectUri), new PlatformParameters(PromptBehavior.SelectAccount));
                            await FetchTokenAsync(authTask).ConfigureAwait(false);
                            break;
                        case 4: // acquire token interactive
                            authTask = context.AcquireTokenAsync(Resource, ClientId, new Uri(RedirectUri), new PlatformParameters(PromptBehavior.Never));
                            await FetchTokenAsync(authTask).ConfigureAwait(false);
                            break;
                        case 5: // acquire token with username and password
                            Console.WriteLine(string.Format(CultureInfo.CurrentCulture, "Enter password for user {0} :", User));
                            authTask = context.AcquireTokenAsync(Resource, ClientId, new UserPasswordCredential(User, Console.ReadLine()));
                            await FetchTokenAsync(authTask).ConfigureAwait(false);
                            break;
                        case 6: // acquire token silent
                            authTask = context.AcquireTokenSilentAsync(Resource, ClientId);
                            await FetchTokenAsync(authTask).ConfigureAwait(false);
                            break;
                        case 7: // device code flow
                            authTask = GetTokenViaDeviceCodeAsync(context);
                            await FetchTokenAsync(authTask).ConfigureAwait(false);
                            break;
                        case 8: // custom httpClient
                            var authResult = await AcquireTokenUsingCustomHttpClientFactoryAsync().ConfigureAwait(false);
                            await FetchTokenAsync(Task.FromResult(authResult)).ConfigureAwait(false);
                            break;
                        case 0:
                            return;
                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                }

                Console.WriteLine("\n\nHit 'ENTER' to continue...");
                Console.ReadLine();
            }
        }

#pragma warning disable CA2201 // Do not raise reserved exception types


        private static async Task<AuthenticationResult> AcquireTokenUsingCustomHttpClientFactoryAsync()
        {
            IHttpClientFactory myHttpClientFactory = new MyHttpClientFactory();
            myHttpClientFactory.GetHttpClient();

            AuthenticationContext authenticationContext = new AuthenticationContext(
                authority: "https://login.microsoftonline.com/common",
                validateAuthority: true,
                tokenCache: TokenCache.DefaultShared, // on .Net and .Net core define your own cache persistence (ommited here)
                httpClientFactory: myHttpClientFactory);

            try
            {
                return await authenticationContext
                    .AcquireTokenSilentAsync(Resource, ClientId)
                    .ConfigureAwait(false);
            }
            catch (AdalSilentTokenAcquisitionException)
            {
                try
                {
                    return await authenticationContext.AcquireTokenAsync(
                        Resource,
                        ClientId,
                        new Uri(RedirectUri),
                        new PlatformParameters(PromptBehavior.SelectAccount))
                        .ConfigureAwait(false);
                    }
                catch (AdalException adalEx)
                {
                    // do not show the error message to the end-user directly
                    if (adalEx is AdalServiceException)
                    {
                        // define your own exception type, do not use ApplicationException
                        throw new ApplicationException(
                            "Authentication failed. Contact your administrator. Internal error message: " + adalEx.Message, adalEx);
                    }

                    throw new ApplicationException(
                           "Authenticaiton failed due to an application problem. Contact your administrator. Internal error message: " + adalEx.Message, adalEx);
                }
                catch (Exception ex)
                {
                    throw new ApplicationException("Internal failure.", ex);
                }

            }

        }

#pragma warning restore CA2201 // Do not raise reserved exception types
        
        private static async Task<AuthenticationResult> GetTokenViaDeviceCodeAsync(AuthenticationContext ctx)
        {
            AuthenticationResult result = null;

            try
            {
                DeviceCodeResult codeResult = await ctx.AcquireDeviceCodeAsync(Resource, ClientId).ConfigureAwait(false);
                Console.ResetColor();
                Console.WriteLine("You need to sign in.");
                Console.WriteLine("Message: " + codeResult.Message + "\n");
                result = await ctx.AcquireTokenByDeviceCodeAsync(codeResult).ConfigureAwait(false);
            }
            catch (Exception exc)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Something went wrong.");
                Console.WriteLine("Message: " + exc.Message + "\n");
            }
            return result;


        }

        private static async Task FetchTokenAsync(Task<AuthenticationResult> authTask)
        {
            await authTask.ConfigureAwait(false);

            Console.BackgroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("Token is {0}", authTask.Result.AccessToken);
            Console.ResetColor();
        }

    }
}