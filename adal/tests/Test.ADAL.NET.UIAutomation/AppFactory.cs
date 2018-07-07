using System;
using Xamarin.UITest;
using Xamarin.UITest.Queries;

namespace Test.ADAL.NET.UIAutomation
{
    /// <summary>
    /// Initializes the app object that represents the main gateway to interact with the app on the device
    /// </summary>
    public class AppFactory
	{
		public static IApp StartApp(Platform platform)
		{
            switch(platform)
            {
                case Platform.Android:
                    return ConfigureApp.Android.InstalledApp("com.Microsoft.XFormsDroid.ADAL").StartApp();
                case Platform.iOS:
                    return ConfigureApp.iOS.StartApp();
                default:
                    return ConfigureApp.Android.InstalledApp("com.Microsoft.XFormsDroid.ADAL").StartApp();
            }
		}
	}
}