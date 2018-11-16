// ------------------------------------------------------------------------------
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
// ------------------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Microsoft.Identity.Client;

namespace Msal.Benchmarks
{
    [SimpleJob(RunStrategy.Throughput, invocationCount: 1000)]
    public class ConfidentialClientAcquireTokenWithCache
    {
        private static readonly TokenCache Cache = new TokenCache();
        private static readonly TokenCache Cache2 = new TokenCache();

        private static readonly ConfidentialClientApplication App = new ConfidentialClientApplication(
            "a40e1db0-b7a2-4e6e-af0e-b4987f73228f",
            "https://login.microsoftonline.com/botframework.com",
            "localhost:8040",
            new ClientCredential("sbF0902^}tyvpvEDXTMX9^|"),
            Cache2,
            Cache);

        private static readonly Semaphore AuthContextSemaphore = new Semaphore(1, 1);

        [Benchmark]
        public async Task AcquireToken()
        {
            await App.AcquireTokenForClientAsync(
                new[]
                {
                    "https://api.botframework.com/.default"
                },
                false).ConfigureAwait(false);
        }
    }
}