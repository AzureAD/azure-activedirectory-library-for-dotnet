using Microsoft.Identity.Labs;

namespace Microsoft.Identity.AutomationTests.SignIn
{
    public interface ISignInFlow
    {
        /// <summary>
        /// The descriptive name of the sign-in flow for logging purposes
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Execute the authentication flow for the given user.
        /// </summary>
        /// <param name="user">The user to sign-in with</param>
        void SignIn(IUser user);
    }
}
