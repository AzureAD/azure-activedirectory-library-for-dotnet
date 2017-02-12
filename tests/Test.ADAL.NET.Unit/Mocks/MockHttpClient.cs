using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.ADAL.NET.Unit.Mocks
{
    internal class MockHttpClient : IHttpClient
    {
        public MockHttpClient()
        {
            this.Headers = new Dictionary<string, string>();
        }

        public IRequestParameters BodyParameters { get; set; }
        public string Accept { get; set; }
        public string ContentType { get; set; }
        public bool UseDefaultCredentials { get; set; }
        public Dictionary<string, string> Headers { get; }

        public IHttpWebResponse Response { get; set; }
        public bool ExpectedUseOfDefaultCredentials { get; set; }

        public Task<IHttpWebResponse> GetResponseAsync()
        {
            Assert.AreEqual(this.ExpectedUseOfDefaultCredentials, this.UseDefaultCredentials);

            return new TaskFactory().StartNew(() => Response);
        }
    }
}
