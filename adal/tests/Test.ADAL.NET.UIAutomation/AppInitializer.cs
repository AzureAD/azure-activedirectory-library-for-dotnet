using System;
using Xamarin.UITest;
using Xamarin.UITest.Queries;

namespace Test.ADAL.NET.UIAutomation
{
	public class AppInitializer
	{
		public static IApp StartApp(Platform platform)
		{
			if (platform == Platform.Android)
			{
				return ConfigureApp.Android.InstalledApp("com.Microsoft.XFormsDroid").StartApp();

            }

			return ConfigureApp.iOS.StartApp();
		}
	}
}