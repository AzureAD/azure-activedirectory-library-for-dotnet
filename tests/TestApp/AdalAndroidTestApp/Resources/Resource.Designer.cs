#pragma warning disable 1591
//------------------------------------------------------------------------------
//
// Copyright (c) Microsoft Corporation.
// All rights reserved.
//
// This code is licensed under the MIT License.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files(the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions :
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//------------------------------------------------------------------------------
//

[assembly: global::Android.Runtime.ResourceDesignerAttribute("AdalAndroidTestApp.Resource", IsApplication=true)]

namespace AdalAndroidTestApp
{
	
	
	[System.CodeDom.Compiler.GeneratedCodeAttribute("Xamarin.Android.Build.Tasks", "1.0.0.0")]
	public partial class Resource
	{
		
		static Resource()
		{
			global::Android.Runtime.ResourceIdManager.UpdateIdValues();
		}
		
		public static void UpdateIdValues()
		{
			global::Microsoft.IdentityModel.Clients.ActiveDirectory.Resource.Id.agentWebView = global::AdalAndroidTestApp.Resource.Id.agentWebView;
			global::Microsoft.IdentityModel.Clients.ActiveDirectory.Resource.Layout.WebAuthenticationBroker = global::AdalAndroidTestApp.Resource.Layout.WebAuthenticationBroker;
			global::Microsoft.IdentityModel.Clients.ActiveDirectory.Resource.String.ApplicationName = global::AdalAndroidTestApp.Resource.String.ApplicationName;
		}
		
		public partial class Attribute
		{
			
			static Attribute()
			{
				global::Android.Runtime.ResourceIdManager.UpdateIdValues();
			}
			
			private Attribute()
			{
			}
		}
		
		public partial class Drawable
		{
			
			// aapt resource value: 0x7f020000
			public const int Icon = 2130837504;
			
			static Drawable()
			{
				global::Android.Runtime.ResourceIdManager.UpdateIdValues();
			}
			
			private Drawable()
			{
			}
		}
		
		public partial class Id
		{
			
			// aapt resource value: 0x7f050004
			public const int accessTokenTextView = 2131034116;
			
			// aapt resource value: 0x7f050001
			public const int acquireTokenInteractiveButton = 2131034113;
			
			// aapt resource value: 0x7f050002
			public const int acquireTokenSilentButton = 2131034114;
			
			// aapt resource value: 0x7f050005
			public const int agentWebView = 2131034117;
			
			// aapt resource value: 0x7f050003
			public const int clearCacheButton = 2131034115;
			
			// aapt resource value: 0x7f050000
			public const int email = 2131034112;
			
			static Id()
			{
				global::Android.Runtime.ResourceIdManager.UpdateIdValues();
			}
			
			private Id()
			{
			}
		}
		
		public partial class Layout
		{
			
			// aapt resource value: 0x7f030000
			public const int Main = 2130903040;
			
			// aapt resource value: 0x7f030001
			public const int WebAuthenticationBroker = 2130903041;
			
			static Layout()
			{
				global::Android.Runtime.ResourceIdManager.UpdateIdValues();
			}
			
			private Layout()
			{
			}
		}
		
		public partial class String
		{
			
			// aapt resource value: 0x7f040000
			public const int ApplicationName = 2130968576;
			
			// aapt resource value: 0x7f040001
			public const int Hello = 2130968577;
			
			static String()
			{
				global::Android.Runtime.ResourceIdManager.UpdateIdValues();
			}
			
			private String()
			{
			}
		}
	}
}
#pragma warning restore 1591
