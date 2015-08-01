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
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Experimental.IdentityModel.Clients.ActiveDirectory;

namespace AdalDesktopTestApp
{
    class Program
    {
        private TokenCache cache = null;

        [STAThread]
        static void Main(string[] args)
        {
            Program program = new Program();
            try
            {
                program.AcquireTokenAsync().Wait();
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
            }
            Console.ReadKey();
        }

        private async Task AcquireTokenAsync()
        {
            cache = new TokenCache(this.ReadCacheFile());
            AuthenticationContext context = new AuthenticationContext("https://login.microsoftonline.com/common/", true, cache);

            IPlatformParameters param = new PlatformParameters(PromptBehavior.Auto, null);
            AuthenticationResult result = await context.AcquireTokenAsync(new[] { "https://outlook.office.com/Mail.Read" }, null,
                "e1eb8a8d-7b0c-4a14-9313-3f2c25c82929", new Uri("urn:ietf:wg:oauth:2.0:oob"), param,
new UserIdentifier("<enter_user_id_here>", UserIdentifierType.RequiredDisplayableId));
            //Console.WriteLine(result.Token + "\n");
            MakeRequest(result.CreateAuthorizationHeader());



            this.WriteCacheFile(cache.Serialize());
        }

        public static void MakeRequest(string token)
        {
            try
            {
                HttpWebRequest request = WebRequest.Create("https://outlook.office365.com/api/v1.0/me/messages") as HttpWebRequest;
                request.Headers.Add("Authorization", token);
                request.Headers.Add("X-ClientService-ClientTag", "Office 365 API Tools 1.1.0612");
                //request.Headers.Add("Accept", "*/*");
                request.Headers.Add("Accept-Charset", "UTF-8");

                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                        throw new Exception(String.Format(
                        "Server error (HTTP {0}: {1}).",
                        response.StatusCode,
                        response.StatusDescription));
                    using (var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                    {
                        string responseText = reader.ReadToEnd();
                        Console.WriteLine(responseText);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
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
