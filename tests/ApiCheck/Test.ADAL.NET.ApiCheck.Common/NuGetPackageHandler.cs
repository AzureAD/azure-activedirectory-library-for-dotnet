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

using NuGet;
using System;
using System.IO;

namespace Test.ADAL.ApiCheck
{
    public class NuGetPackageHandler
    {
        private readonly IPackageRepository remoteRepository;
        private readonly string localCacheRoot;
        private readonly Action<string> logAction;
        private readonly IPackageManager packageManager;

        public NuGetPackageHandler(IPackageRepository remoteRepository, string localCacheRoot, Action<string> logAction)
        {
            if (remoteRepository == null)
            {
                throw new ArgumentNullException("remoteRepository");
            }
            if (string.IsNullOrWhiteSpace(localCacheRoot))
            {
                throw new ArgumentNullException("localPackageDestination");
            }
            if (logAction == null)
            {
                throw new ArgumentNullException("logAction");
            }

            this.logAction = logAction;
            this.remoteRepository = remoteRepository;
            this.localCacheRoot = localCacheRoot;

            Directory.CreateDirectory(localCacheRoot);
            this.packageManager = new PackageManager(remoteRepository, localCacheRoot)
            {
                Logger = new NuGetLoggerTestAdapter(logAction)
            };
        }

        #region INuGetPackageHandler

        /// <summary>
        /// Attempts to download a NuGet package with the specified id and version constraint
        /// </summary>
        /// <returns>The absolute path to the package</returns>
        public string FindAndDownloadPackage(string packageId, IVersionSpec version)
        {
            if (string.IsNullOrWhiteSpace(packageId))
            {
                throw new ArgumentNullException("packageId");
            }

            IPackage package = FindPackage(this.remoteRepository, packageId, version);

            return InstallPackage(package);
        }

        #endregion

        private IPackage FindPackage(IPackageRepository repository, string packageId, IVersionSpec versionSpec)
        {
            IPackage package = PackageRepositoryExtensions.FindPackage(repository, packageId, versionSpec, false, true);

            if (package == null)
            {
                throw new InvalidOperationException("Package not found");
            }

            return package;
        }

        private string InstallPackage(IPackage package)
        {
            string existingPackagePath = GetInstallLocation(package);

            try
            {
                // Prerelease packages enabled by default
                this.packageManager.InstallPackage(package, true, true, false);
            }
            catch (InvalidOperationException e)
            {
                try
                {
                    Directory.Delete(existingPackagePath, true);
                }
                catch { }
                throw new InvalidOperationException($"Cannot install package ${package.GetFullName()} - root cause: ${e.Message}");
            }
            finally
            {

            }

            return existingPackagePath;
        }

        private string GetInstallLocation(IPackage package)
        {
            return Path.Combine(this.localCacheRoot, package.Id + "." + package.Version.ToString(), "lib");
        }
    }
}
