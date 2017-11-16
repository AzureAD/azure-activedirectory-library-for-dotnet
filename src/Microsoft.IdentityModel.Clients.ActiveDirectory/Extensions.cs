using System;
using System.Globalization;
using System.Text;

namespace Microsoft.IdentityModel.Clients.ActiveDirectory
{
    internal static class Extensions
    {
        internal static string GetPiiScrubbedDetails(this Exception ex)
        {
            string res = null;
            if (ex != null)
            {
                var sb = new StringBuilder();

                sb.Append(string.Format(CultureInfo.CurrentCulture, "Exception type: {0}", ex.GetType()));

                if (ex is AdalException)
                {
                    var adalException = (AdalException) ex;
                    sb.Append(string.Format(CultureInfo.CurrentCulture, ", ErrorCode: {0}", adalException.ErrorCode));
                }

                if (ex is AdalServiceException)
                {
                    var adalServiceException  = (AdalServiceException) ex; 
                    sb.Append(string.Format(CultureInfo.CurrentCulture, ", StatusCode: {0}", adalServiceException.StatusCode));
                }

                if (ex.InnerException != null)
                {
                    sb.Append(" ---> " + GetPiiScrubbedDetails(ex.InnerException) + Environment.NewLine +
                              "--- End of inner exception stack trace ---");
                }
                if (ex.StackTrace != null)
                {
                    sb.Append(Environment.NewLine + ex.StackTrace);
                }

                res = sb.ToString();
            }

            return res;
        }
    }
}