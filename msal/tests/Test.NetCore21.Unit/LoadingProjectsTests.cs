using Microsoft.Identity.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NetCoreUTest
{
    [TestClass]
    public class LoadingProjectsTests
    {
        [TestMethod]
        public void CanDeserializeTokenCacheInNetCore()
        {
            TokenCache tokenCache = new TokenCache();
            tokenCache.Deserialize(null);
        }
    }
}
