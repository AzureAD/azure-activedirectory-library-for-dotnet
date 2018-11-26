namespace Microsoft.Identity.AutomationTests.Model
{
    public class Resource
    {
        /// <summary>
        /// URL from where a token can be asked
        /// </summary>
        public string RequestUrl { get; set; }

        /// <summary>
        /// A Url for which authentication is needed
        /// </summary>
        public string ProtectedUrl { get; set; }
    }
}