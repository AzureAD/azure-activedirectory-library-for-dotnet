using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Identity.AutomationTests
{
    public abstract class ResponseBase
    {
        internal IReadOnlyDictionary<string, object> _rawResponseDictionary;

        public string Response { get; }

        public string ResultLogs { get; }

        protected ResponseBase(string response, string resultLogs)
        {
            Response = response;
            ResultLogs = resultLogs;
        }

        public IReadOnlyDictionary<string, object> GetResponseAsDictionary()
        {
            if (_rawResponseDictionary == null)
            {
                _rawResponseDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(Response, new JsonSerializerSettings
                {
                    DateTimeZoneHandling = DateTimeZoneHandling.Utc // Keep time zone as UTC
                });
            }

            return _rawResponseDictionary;
        }

        public string GetPropertyAsString(string key)
        {
            var dict = GetResponseAsDictionary();
            object value;
            dict.TryGetValue(key, out value);
            return value?.ToString();
        }

        public string GetErrorMessage() => GetPropertyAsString("error");

        public bool IsSuccess => !GetResponseAsDictionary().ContainsKey("error");
    }
}
