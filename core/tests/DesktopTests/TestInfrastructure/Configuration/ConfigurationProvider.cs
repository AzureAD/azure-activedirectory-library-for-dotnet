using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.AutomationTests.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Microsoft.Identity.AutomationTests.Configuration
{
    public sealed class ConfigurationProvider
    {
        // location of the build artifacts - these need to be in sync with the VSTS build definition; {0} = the build number
        private const string OlympusDropForAndroid = @"\\olympusshare\Olympus\apps\DevEx\android\{0}\apk\automationtestapp-debug.apk";
        private const string OlympusDropForIos = @"\\olympusshare\Olympus\apps\DevEx\ios\{0}\Release-iphoneos\ADALAutomation.app";
        private const string OlympusDropForAdalDotNet = @"\\olympusshare\Olympus\apps\DevEx\dotnet\adal\{0}\adalApp\WinFormsAutomationApp.exe";

        private const string ApplicationsConfig = "Microsoft.Identity.AutomationTests.resources.applications.json";
        private const string ResourcesConfigFile = "Microsoft.Identity.AutomationTests.resources.resources.json";
        private const string BlacklistConfigFile = "Microsoft.Identity.AutomationTests.resources.blacklist.json";

        private IConfigurationRoot _config;

        private IDictionary<string, Application> _appConfig;
        private IDictionary<string, Resource> _resourcesConfig;

        //private readonly ILabService _labService;

        // Pulled from the Appium and Olympus device capability lists: 
        // https://github.com/appium/appium/blob/master/docs/en/writing-running-appium/caps.md
        // http://sharepoint/sites/bingclients/ARIAClients/_layouts/15/WopiFrame2.aspx?sourcedoc=%7Be49d0fb2-df5e-4448-a7ef-6e32c065520e%7D&action=edit&wd=target%28%2FDesired%20Capabilities%20Cheat-sheet%7C2a8a2807-e564-48ad-b0ea-e6c22b267d21%2F%29
        private static readonly string[] EnvVarDeviceCapabilityWhitelist =
        {
            // Appium
            "app",
            "deviceName",
            "platform",
            "platformName",
            "automationName",
            // Olympus
            "olympusDevice",
            "olympusDeviceCategory",
            "olympusDeviceModel",
            "olympusDeviceOS",
            "olympusDevicePlatform",
            "olympusDeviceTag",
            "olympusPriority",
            "olympusSessionId",
            "olympusTeam",
            "olympusTestName",
            // Android
            "appActivity",
            "appPackage",
            "recreateChromeDriverSessions",
            "autoGrantPermissions",
            // iOS
            "bundleId",
            "appName"
        };


        #region Singleton

        public static readonly ConfigurationProvider Instance = new ConfigurationProvider();

        private ConfigurationProvider()
        {
            this.ParseConfigFiles();

            var keyVaultConfig = _config.GetSection("keyVault");
            if (keyVaultConfig == null)
            {
                throw new InvalidOperationException("Could not find the 'keyVault' configuration section");
            }

            KeyVaultSecretsProvider.Initialize(keyVaultConfig);

            Assembly assembly = Assembly.GetExecutingAssembly();

            //_labService = new LabServiceApi();

        }

        #endregion

        public string GetSetting(string key)
        {
            return _config[key];
        }

        public T GetSetting<T>(string key)
        {
            return _config.GetValue<T>(key);
        }

        public string GetSettingOrDefault(string key, string defaultValue)
        {
            var value = GetSetting(key);
            return value ?? defaultValue;
        }

        public T GetSettingOrDefault<T>(string key, T defaultValue)
        {
            var value = GetSetting<T>(key);
            return value.Equals(default(T)) ? defaultValue : value;
        }

        /// <summary>
        /// Returns a dictionary of settings based on the device.json files, that can be used to configure Appium / Olympus
        /// or a local simulator
        /// </summary>
        public DeviceConfiguration GetDeviceConfiguration(PlatformType platformType)
        {
           return new DeviceConfiguration(_config.GetSection(platformType.ToString()));
        }

        public Resource GetResource(ResourceType resourceType)
        {
            return _resourcesConfig?[resourceType.ToString()];
        }

        public Application GetApplication(ApplicationType applicationType)
        {
            return _appConfig?[applicationType.ToString()];
        }

        ///// <summary>
        ///// Returns a test user account for use in testing.
        ///// An exception is thrown if no matching user is found.
        ///// </summary>
        ///// <param name="query">Any and all parameters that the returned user should satisfy.</param>
        ///// <returns>A single user that matches the given query parameters.</returns>
        //public IUser GetUser(UserQueryParameters query)
        //{
        //    var availableUsers = GetUsers(query);
        //    Assert.AreNotEqual(0, availableUsers.Count(), "Found no users for the given query.");
        //    return availableUsers.First();
        //}

        ///// <summary>
        ///// Returns a test user account for use in testing.
        ///// </summary>
        ///// <param name="query">Any and all parameters that the returned user should satisfy.</param>
        ///// <returns>Users that match the given query parameters.</returns>
        //public IEnumerable<IUser> GetUsers(UserQueryParameters query)
        //{
        //    foreach (var user in _labService.GetUsers(query))
        //    {
        //        if (!Uri.IsWellFormedUriString(user.CredentialUrl, UriKind.Absolute))
        //        {
        //            Console.WriteLine($"User '{user.Upn}' has invalid Credential URL: '{user.CredentialUrl}'");
        //            continue;
        //        }

        //        if (user.IsExternal && user.HomeUser == null)
        //        {
        //            Console.WriteLine($"User '{user.Upn}' has no matching home user.");
        //            continue;
        //        }

        //        yield return user;
        //    }
        //}

        private void ParseConfigFiles()
        {
            BuildConfig();

            Assembly assembly = Assembly.GetExecutingAssembly();

            using (var stream = assembly.GetManifestResourceStream(ResourcesConfigFile))
            using (var streamReader = new StreamReader(stream))
            {
                var json = streamReader.ReadToEnd();
                var dict = JsonConvert.DeserializeObject<Dictionary<string, Resource>>(json);
                _resourcesConfig = new Dictionary<string, Resource>(dict, StringComparer.OrdinalIgnoreCase);
            }

            using (var stream = assembly.GetManifestResourceStream(ApplicationsConfig))
            using (var streamReader = new StreamReader(stream))
            {
                var json = streamReader.ReadToEnd();
                var dict = JsonConvert.DeserializeObject<Dictionary<string, Application>>(json);
                _appConfig = new Dictionary<string, Application>(dict, StringComparer.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Loads the main configuration of the tests, which is different depending on running locally or in VSTS.
        /// Users can also edit config.local.json and use environment variables for further customization.
        /// The location of the app under test is read from build variables for VSTS.
        /// </summary>
        private void BuildConfig()
        {
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json");

            if (IsVstsBuild)
            {
                // inject the location of the app under test in the configuration
                string buildNumber = Environment.GetEnvironmentVariable("BUILD_BUILDNUMBER");

                if (string.IsNullOrWhiteSpace(buildNumber))
                {
                    throw new InvalidOperationException("Cannot determine the build number. Enviroment variable BUILD_BUILDNUMBER not found.");
                }

                var vstsAppDropCapabilities = new Dictionary<string, string>
                    {
                        {"android:deviceCapabilities:app", string.Format(OlympusDropForAndroid, buildNumber)},
                        {"ios:deviceCapabilities:app", string.Format(OlympusDropForIos, buildNumber)},
                        {"desktop:deviceCapabilities:app", string.Format(OlympusDropForAdalDotNet, buildNumber)}   
                    };

                configBuilder.AddInMemoryCollection(vstsAppDropCapabilities)
                             .AddJsonFile("config.vsts.json");
            }
            else
            {
                configBuilder.AddJsonFile("config.local.json");
            }

            var environmentVariableConfig = GetEnvironmentVariableConfig();

            this._config = configBuilder.AddInMemoryCollection(environmentVariableConfig)
                                        .Build();
        }

        /// <summary>
        /// Builds a config from environment variables which will override anything set by files.
        /// Such variables are ALL UPPERCASE, so a certain subset is mapped back to camelCase.
        /// </summary>
        private IReadOnlyDictionary<string, string> GetEnvironmentVariableConfig()
        {
            IDictionary<string, string> environmentVariables = Environment.GetEnvironmentVariables().ToStringDictionary();

            // Replace the keys for any matching upper-case environment variables with ones with the correct case
            foreach (string varName in GenerateWhitelistEnvironmentVars())
            {
                string upperVarName = varName.ToUpperInvariant();
                string varValue;
                if (environmentVariables.TryGetValue(upperVarName, out varValue))
                {
                    environmentVariables.Remove(upperVarName);
                    environmentVariables.Add(varName, varValue);
                }
            }

            return new ReadOnlyDictionary<string, string>(environmentVariables);
        }

        /// <summary>
        /// Returns a list of all full property names covered by the whitelist, each in the form:
        /// [platformName].deviceCapabilities.[propertyName]
        /// </summary>
        private IEnumerable<string> GenerateWhitelistEnvironmentVars()
        {
            foreach (var whitelistEntry in EnvVarDeviceCapabilityWhitelist)
            {
                // One entry for each platform
                foreach (PlatformType automationType in Enum.GetValues(typeof(PlatformType)))
                {
                    var platformName = automationType.ToString().ToLowerInvariant();

                    // Looks like: android:deviceCapabilities:deviceName
                    yield return $"{platformName}:deviceCapabilities:{whitelistEntry}";
                }
            }
        }

       public static bool IsVstsBuild => !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("BUILD_BUILDURI"));
    }
}
