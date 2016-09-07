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
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.IO;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace AdalDesktopTestApp
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                AcquireTokenAsync().Wait();
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

        private static async Task AcquireTokenAsync()
        {
            Microsoft.IdentityModel.Clients.ActiveDirectory.Telemetry telemetry =
Microsoft.IdentityModel.Clients.ActiveDirectory.Telemetry.GetInstance();
            DispatcherImplement dispatcher = new DispatcherImplement();
            telemetry.RegisterDispatcher(dispatcher, true);

            //LoggerCallbackHandler.Callback = new MyCallback();

            AuthenticationContext context = new AuthenticationContext("https://login.microsoftonline.com/common", true);

            var result = await context.AcquireTokenAsync("https://graph.windows.net", "193faa18-0c0b-45f3-9125-b08ff04d9890", new UserPasswordCredential("test@abgun.onmicrosoft.com", "P@ssword<"));
            TokenCache.DefaultShared.Clear();
            string token = result.AccessToken;
            Console.WriteLine(token + "\n");
            dispatcher.file();
        }
    }

    internal class DispatcherImplement : IDispatcher
    {
        private readonly List<List<Tuple<string, string>>> storeList = new List<List<Tuple<string, string>>>();

        void IDispatcher.Dispatch(List<Tuple<string, string>> Event)
        {
            storeList.Add(Event);
        }

        public int Count
        {
            get { return storeList.Count; }
        }

        public void clear()
        {
            storeList.Clear();
        }

        public void file()
        {
            using (TextWriter tw = new StreamWriter("C:/Users/abgun/test.txt"))
            {
                foreach (List<Tuple<string, string>> list in storeList)
                {
                    foreach (Tuple<string, string> tuple in list)
                    {
                        tw.WriteLine(tuple.Item1 + " " + tuple.Item2 + "\r\n");
                    }
                }
            }
        }
    }

    internal class MyCallback : IAdalLogCallback
        {
            public void Log(LogLevel level, string message)
            {
                Console.WriteLine(level + " - " + message);
            }
        }
    }
