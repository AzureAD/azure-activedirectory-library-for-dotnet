using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace AutomationApp
{
    public partial class MainPage : ContentPage
    {
        
        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnAcquireTokenClicked(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new DataInputPage(AuthenticationHelper.AcquireToken));
        }

        private async void OnAcquireTokenSilentClicked(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new DataInputPage(AuthenticationHelper.AcquireTokenSilent));
        }
    }
}
