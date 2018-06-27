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

using ApiCheck;
using ApiCheck.Configuration;
using ApiCheck.Loader;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Reflection;
using NuGet;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Test.ADAL.ApiCheck
{
    /// <summary>
    /// This wrapper is needed because we cannot Assembly.Load an the "same" assembly (built for different platforms)
    /// into the same AppDomain and the underlying comparison tool does with external AppDomains.
    /// However, mstest invokes a new executing process for each test assembly, so there are no duplicate assemblies.
    /// </summary>
    public class ApiCheckTestWrapper
    {
        private readonly string tfm;
        private readonly string packageId;
        private readonly string minInclusiveVersion;
        private readonly string maxExclusiveVersion;
        private readonly TestContext testContext;

        /// <summary>
        /// A wrapper for the test
        /// </summary>
        /// <param name="testContext">the mstest context</param>
        /// <param name="tfm">the target framework moniker, e.g. "net45"</param>
        /// <param name="packageId">the name of the package as it appears on NuGet</param>
        /// <param name="minInclusiveVersion">the min version of the package on NuGet</param>
        /// <param name="maxExclusiveVersion">the max version of the package on NuGet</param>
        public ApiCheckTestWrapper(
            TestContext testContext, 
            string tfm, 
            string packageId, 
            string minInclusiveVersion, 
            string maxExclusiveVersion )
        {
            this.testContext = testContext;
            this.tfm = tfm;
            this.packageId = packageId;
            this.minInclusiveVersion = minInclusiveVersion;
            this.maxExclusiveVersion = maxExclusiveVersion;
        }

        /// <summary>
        /// This test checks backwards compatibility of the public API between: 
        /// the latest 3.x.x version of ADAL as found on public NuGet
        /// the developer version which is just built
        /// 
        /// Assemblies from all target frameworks are compared. 
        /// 
        /// In case discrepancies are found, have a look at the reports attached in the test output. You can also
        /// manually inspect the 2 assemblies with the excellent ApiReviewer tool from the .net team found at \\fxcore\tools
        /// </summary>
        public void RunTest()
        {
            string devFolder = GetDevProductFolder(packageId);
            string refFolder = DownloadRefProductFromNuget(packageId, this.minInclusiveVersion, this.maxExclusiveVersion);

            // see https://github.com/PMudra/ApiCheck for how to configure exceptions
            IEnumerable<string> exceptions = Enumerable.Empty<string>();

            RunTestInternal(devFolder, refFolder, exceptions);
        }

        private void RunTestInternal(string devFolder, string refFolder, IEnumerable<string> exceptions)
        {
            string refDllPath = Path.Combine(refFolder, this.tfm, packageId + ".dll");
            string devDllPath = Path.Combine(devFolder, this.tfm, packageId + ".dll");

            string xmlReport = RunAssemblyChecker(refDllPath, devDllPath, exceptions);
            AssertApiCheckResults(xmlReport);
        }

        private string DownloadRefProductFromNuget(string packageId, string minInclusiveVersion, string maxExclusiveVersion)
        {

            string downloadDir = Path.Combine(testContext.DeploymentDirectory, ".nuget");
            IVersionSpec versionSpec = new VersionSpec()
            {
                MinVersion = SemanticVersion.Parse(minInclusiveVersion),
                IsMinInclusive = true,
                MaxVersion = SemanticVersion.Parse(maxExclusiveVersion),
                IsMaxInclusive = false
            };

            NuGetPackageHandler nuGetPackageHandler = new NuGetPackageHandler(
                PackageRepositoryFactory.Default.CreateRepository("https://packages.nuget.org/api/v2"),
                downloadDir,
                (msg) => testContext.WriteLine(msg));

            return nuGetPackageHandler.FindAndDownloadPackage(packageId, versionSpec);
        }

        private string RunAssemblyChecker(string refAssemblyPath, string devAssemblyPath, IEnumerable<string> ignoredChanges)
        {
            string xmlReportFile = Path.Combine(testContext.TestResultsDirectory, "api_check_report.xml");
            string htmlReportFile = Path.Combine(testContext.TestResultsDirectory, "api_check_report.html");

            using (AssemblyLoader assemblyLoader = new AssemblyLoader())
            {
                //  using the included AssemblyLoader that automatically resolves dependencies
                Assembly refAssembly = assemblyLoader.ReflectionOnlyLoad(refAssemblyPath);
                Assembly devAssembly = assemblyLoader.ReflectionOnlyLoad(devAssemblyPath);

                // the comparer configuration specifies the severity levels of the changed API elements and which elements to ignore
                ComparerConfiguration configuration = new ComparerConfiguration();

                // all listed elements are not compared
                if (ignoredChanges != null && ignoredChanges.Any())
                {
                    configuration.Ignore.AddRange(ignoredChanges);
                }

                // override the default severities
                var scenarioList = new List<string>() { "ChangedAttribute", "ChangedElement", "RemovedElement" };

                // check the scenarios that we might have a breaking change
                using (var xmlStream = new FileStream(xmlReportFile, FileMode.Create))
                using (var htmlStream = new FileStream(htmlReportFile, FileMode.Create))
                {
                    ApiComparer.CreateInstance(refAssembly, devAssembly)
                          // configure the logging and the comparer 
                          .WithComparerConfiguration(configuration)
                          // write report to desired streams
                          .WithHtmlReport(htmlStream)
                          .WithXmlReport(xmlStream)
                          .Build()     // creating the ApiChecker
                          .CheckApi(); // doing the comparison
                }

                testContext.AddResultFile(htmlReportFile);
                testContext.AddResultFile(xmlReportFile);

                return xmlReportFile;
            }
        }

        private void AssertApiCheckResults(string reportPath)
        {
            var scenarioList = new List<string>() { "ChangedAttribute", "ChangedElement", "RemovedElement" };

            // check the scenarios that we might have a breaking change
            foreach (var scenario in scenarioList)
            {
                XElement doc = XElement.Load(reportPath);

                foreach (XElement change in doc.Descendants(scenario))
                {
                    if (change.Attribute("Severity") != null &&
                        "Error".Equals(change.Attribute("Severity").Value))
                    {
                        Assert.Fail("Api Change detected a breaking change. An html report is attached to the test output.");
                    }
                }
            }
        }

        private string GetBuildFlavour()
        {
#if DEBUG
            return "Debug";
#else
            return "Release";
#endif
        }

        private string GetDevProductFolder(string productName)
        {
            string path = Path.Combine(testContext.TestDir, "..", "..", "src", productName, "bin", GetBuildFlavour());
            return path;
        }

        private IEnumerable<string> EnumerateTargetFrameworks(string productDir)
        {
            return Directory.EnumerateDirectories(productDir).Select(wTfm => Path.GetFileName(wTfm));
        }
    }
}
