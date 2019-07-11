//----------------------------------------------------------------------
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

using System.Collections.Generic;

namespace XFormsApp
{
    public static class AppConstants
    {
        //Applications
        public const string UiAutomationTestClientId = "3c1e0e0d-b742-45ba-a35e-01c664e14b16";
        public const string MSIDLAB4ClientId = "4b0db8c2-9f26-4417-8bde-3f0e3656f8e0";
        public const string ManualTestClientId = "d3590ed6-52b3-4102-aeff-aad2292ab01c";
        public const string BrokerClientId = "3a981c29-5df7-4656-a776-c473e132a0d4";

        //Resources
        public const string UiAutomationTestResource = "ae55a6cc-da5e-42f8-b75d-c37e41a1a0d9";
        public const string MSGraph = "https://graph.microsoft.com";
        public const string Exchange = "https://outlook.office365.com/";
        public const string SharePoint = "https://microsoft.sharepoint-df.com/ ";

        static AppConstants()
        {
            //Adding default applications and resources to make testing easier by removing the need to rebuild the application 
            //whenever a user wants to change a resource. You can add new applications and resources here and they will be available via 
            //drop down when the app runs.
            LabelToApplicationUriMap = new Dictionary<string, string>
            {
                { "Ui Test App", UiAutomationTestClientId },
                { "MSID Lab 4", MSIDLAB4ClientId },
                { "Manual Test", ManualTestClientId },
                { "Broker Test", BrokerClientId }
            };

            LabelToResourceUriMap = new Dictionary<string, string>
            {
                { "MS Graph", MSGraph },
                { "Ui Test Resource", UiAutomationTestResource },
                { "Exchange", Exchange },
                { "SharePoint", SharePoint }
            };

            PromptBehaviorList = new List<string>
            {
                "auto",
                "always"
            };
        }

        public static Dictionary<string, string> LabelToApplicationUriMap { get; set; }
        public static Dictionary<string, string> LabelToResourceUriMap { get; set; }
        public static List<string> PromptBehaviorList { get; set; }
    }
}
