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
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace AdalCoreCLRTestApp
{
    class Program
    {

        //private static string AUTHORITY = "https://login.microsoftonline.com/microsoft.onmicrosoft.com";
        //private static string CLIENT_ID = "6d742d8f-5afd-4a88-b003-ae760656b0da";

        private static string AUTHORITY = "https://login.microsoftonline.com/pesomka.onmicrosoft.com";
        private static string CLIENT_ID = "07750a6f-a05a-411d-bac4-92e1ef446da3";

        private static string RESOURCE = "https://graph.windows.net";

        static void Main(string[] args)
        {
            try
            {
                AcquireTokenUsingIntegAuthFlow().Wait();
            }
            catch (AggregateException ae)
            {
                Console.WriteLine(ae.InnerException.Message);
                Console.WriteLine(ae.InnerException.StackTrace);
            }
            finally
            {
                Console.ReadKey();
            }
        }

        private static async Task AcquireTokenUsingIntegAuthFlow()
        {
            AuthenticationContext context = new AuthenticationContext(AUTHORITY, true);
            try
            {
                var result = await context.AcquireTokenAsync(RESOURCE, CLIENT_ID, new UserCredential("pesomka@microsoft.com"));

               result = await context.AcquireTokenAsync(RESOURCE, CLIENT_ID, new Uri("http://local"), null);


                result = await context.AcquireTokenSilentAsync(RESOURCE, CLIENT_ID);

                Console.WriteLine(result.AccessToken + "\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

            }
        }

        private static async Task AcquireTokenUsingDeviceCodeFlow()
        {
            AuthenticationContext context = new AuthenticationContext(AUTHORITY, true);
            //var certificate = GetCertificateByThumbprint("<CERT_THUMBPRINT>");
            DeviceCodeResult codeResult;
            try
            {
                codeResult = await context.AcquireDeviceCodeAsync(RESOURCE, CLIENT_ID);
                string msg = codeResult.Message;

                Console.WriteLine(msg + "\n");

                var result = await context.AcquireTokenByDeviceCodeAsync(codeResult);


                Console.WriteLine(result.AccessToken + "\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

            }
        }

        private static X509Certificate2 GetCertificateByThumbprint(string thumbprint)
        {
            using (var store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
            {
                store.Open(OpenFlags.ReadOnly);
                var certs = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
                if (certs.Count > 0)
                {
                    return certs[0];
                }
                throw new Exception($"Cannot find certificate with thumbprint '{thumbprint}'");
            }
        }
    }
}
