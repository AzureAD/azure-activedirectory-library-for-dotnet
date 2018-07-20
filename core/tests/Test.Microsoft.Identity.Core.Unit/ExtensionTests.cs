using Microsoft.Identity.Core.Exceptions;
using Microsoft.Identity.Core.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.Microsoft.Identity.Core.Unit
{
    [TestClass]
    public class ExtensionTests
    {
        [TestMethod]
        public void GetCorePiiScrubbedDetails_CoreClient()
        {
            // Arrange
            string exMessage = "exMessage";
            string exCode = "exCode";
            string piiMessage = "";

            try
            {
                // throw it to have a stack trace
                throw new CoreClientException(exCode, exMessage);
            }
            catch (Exception e)
            {
                // Act
                piiMessage = e.GetCorePiiScrubbedDetails();
            }

            // Assert
            Assert.IsFalse(String.IsNullOrEmpty(piiMessage));
            Assert.IsTrue(
                piiMessage.Contains(typeof(CoreClientException).Name),
                "The pii message should contain the exception type");
            Assert.IsTrue(piiMessage.Contains(exCode));
            Assert.IsFalse(piiMessage.Contains(exMessage));
            Assert.IsTrue(piiMessage.Contains(":line"), "Should have the stack trace");
        }

        [TestMethod]
        public void GetCorePiiScrubbedDetails_CoreServices()
        {
            // Arrange
            string exMessage = "exMessage";
            string exCode = "exCode";
            int exStatus = 500;
            string innerMessage = "innerMessage";
            string piiMessage = "";

            var exception = new CoreServiceException(
                exCode,
                exMessage,
                new NotImplementedException(innerMessage))
            {
                StatusCode = exStatus
            };

            // Act
            piiMessage = exception.GetCorePiiScrubbedDetails();

            // Assert
            Assert.IsFalse(String.IsNullOrEmpty(piiMessage));
            Assert.IsTrue(
                piiMessage.Contains(typeof(CoreServiceException).Name),
                "The pii message should contain the exception type");
            Assert.IsTrue(piiMessage.Contains(exCode));
            Assert.IsTrue(piiMessage.Contains(exStatus.ToString()));
            Assert.IsFalse(piiMessage.Contains(exMessage));

            Assert.IsTrue(
                piiMessage.Contains(typeof(NotImplementedException).Name),
                "The pii message should contain the inner exception type");
            Assert.IsFalse(piiMessage.Contains(innerMessage));

        }
    }
}
