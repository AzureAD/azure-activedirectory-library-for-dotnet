using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UniversalApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void accessTokenButton_Click(object sender, RoutedEventArgs e)
        {
            AuthenticationContext ctx = new AuthenticationContext("https://login.microsoftonline.com/common");
            try
            {
                AuthenticationResult result = await ctx.AcquireTokenAsync("https://graph.microsoft.com", "<CLIENT_ID>",
                    new Uri("<REDIRECT_URI>"));

                resultTextbox.Text = result.AccessToken;
            }
            catch (Exception exc)
            {
                resultTextbox.Text = exc.Message;
            }
        }

        private async void conditionalAccessButton_Click(object sender, RoutedEventArgs e)
        {
            string claims = "{\"access_token\":{\"polids\":{\"essential\":true,\"values\":[\"5ce770ea-8690-4747-aa73-c5b3cd509cd4\"]}}}";

            AuthenticationContext ctx = new AuthenticationContext("https://login.microsoftonline.com/common");
            try
            {
                AuthenticationResult result = await ctx.AcquireTokenAsync("https://graph.microsoft.com", "<CLIENT_ID>",
                   new Uri("<REDIRECT_URI>"), PromptBehavior.Auto, new UserIdentifier("<USER>", UserIdentifierType.OptionalDisplayableId), null, claims);

                resultTextbox.Text = result.AccessToken;
            }
            catch (Exception exc)
            {
                resultTextbox.Text = exc.Message;
            }
        }
    }
}
