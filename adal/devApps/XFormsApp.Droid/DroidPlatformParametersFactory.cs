using Android.App;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using Xamarin.Forms;
using XFormsApp.Droid;
using Application = Android.App.Application;

namespace XFormsApp.Droid
{
    public class DroidPlatformParametersFactory : IPlatformParametersFactory
    {
        // Workaround for DependencyService not being able to attach a pre-made object to an interface
        // In a production app I would consider using a proper dependency service like AutoFac
        // as Xamarin.DependencyService is just a simple service locator
        public static Activity Activity { get; set; }

        public IPlatformParameters GetPlatformParameters(string promptBehavior)
        {
            if (Activity == null)
            {
                throw new InvalidOperationException("Please initialize Activity");
            }

            switch (promptBehavior)
            {
                case "always":
                    return new PlatformParameters(Activity, false, PromptBehavior.Always);
                default:
                    return new PlatformParameters(Activity, false, PromptBehavior.Auto);
            }
        }
    }
}