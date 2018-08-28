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
            Assert.IsFalse(tokenCache.HasStateChanged, "State should not have changed when deserializing nothing.");
        }
    }
}
