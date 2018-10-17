#pragma once
#include "ctmacros.hpp"
#include "ILogger.hpp"

#include <string>
#include <vector>
#include <map>

namespace Microsoft { namespace Applications { namespace Experimentation { namespace ECS {

    /// <summary>
    /// The ECSClientConfiguration structure configures the ECSClient.
    /// </summary>
    struct ECSClientConfiguration
    {
         /// <summary>
        /// [required]A string that contains the name of the ECS client associated with the configurations.
        /// </summary>
        std::string clientName;

        /// <summary>
        /// [required] A string that contains the version of the ECS client associated with the configurations.
        /// </summary>
        std::string clientVersion;

        /// <summary>
        /// [required] A string that contains the fully-qualified path name of the file that the ECS client 
        /// uses to cache the configuration.
        /// </summary>
        std::string cacheFilePathName;

        /// <summary>
        /// [optional] An unsigned integer that contains the default time (in minutes) to expire the cached configuration.
        /// </summary>
        unsigned int defaultExpiryTimeInMin;

        /// <summary>
        /// [optional] A standard vector of strings that contains the ECS server URIs.
        /// If you don't specify a value, then the default value is used.
        /// </summary>
        std::vector<std::string> serverUrls;

        // [optional] enabled ECS telemetry
        bool enableECSClientTelemetry = false;
    };

    /// <summary>
    /// The IECSClientCallback class contains the callback interface for the ECS client to 
    /// notify its listener of events when they occur.
    /// </summary>
    class IECSClientCallback
    {
    public:
        /// <summary>
        /// The ECSClientEventType enumeration contains a set of values that specify the outcome of a configuration update.
        /// </summary>
        enum ECSClientEventType
        {
            /// <summary>
            /// The configuration update succeeded.
            /// </summary>
            ET_CONFIG_UPDATE_SUCCEEDED = 0,
            
            /// <summary>
            /// The configuration update failed.
            /// </summary>
            ET_CONFIG_UPDATE_FAILED
        };

        /// <summary>
        /// The ECSClientEventContext structure contains the state of the ECS client event context.
        /// </summary>
        struct ECSClientEventContext
        {
            /// <summary>
            /// A string that contains the name of the ECS client.
            /// </summary>
            std::string clientName;

            /// <summary>
            /// A string that contains the ECS client version.
            /// </summary>
            std::string clientVersion;

            /// <summary>
            /// A string that contains the user ID.
            /// </summary>
            std::string userId;

            /// <summary>
            /// A string that contains the device ID.
            /// </summary>
            std::string deviceId;

            /// <summary>
            /// A key-value map that contains the list of request parameters.
            /// </summary>
            std::map<std::string, std::string> requestParameters;

            /// <summary>
            /// An unsigned integer that contains the configured expiry time in seconds.
            /// </summary>
            unsigned int configExpiryTimeInSec;

            /// <summary>
            /// A boolean that indicates whether the ECS configuration was updated from ECS.
            /// </summary>
            bool configUpdateFromECS;
        };

        /// <summary>
        /// A callback method for the ECSClient to notify its listener of configuration changes.
        /// </summary>
        /// <param name="evtType">The type of the ECSClient event, specified using 
        /// one of the ECSClientEventType enumeration values.</param>
        /// <param name="evtContext">The context information of the ECSClient event, expressed as an ECSClientEventContext object.</param>
        virtual void OnECSClientEvent(ECSClientEventType evtType, ECSClientEventContext evtContext) = 0;
    };

    /// <summary>
    /// The IECSClient class is the interface to an ECS client.
    /// </summary>
    class ARIASDK_LIBABI IECSClient
    {
    public:
        /// <summary>
        /// Creates a new instance of an IECSClient.
        /// </summary>
        static IECSClient* ARIASDK_LIBABI_CDECL CreateInstance();

        /// <summary>
        /// Destroys the specified IECSClient instance.
        /// </summary>
        static void ARIASDK_LIBABI_CDECL DestroyInstance(IECSClient** ppECSClient);

        /// <summary>
        /// Initializes the IECSClient with the specified configuration.
        /// </summary>
        /// <param name="config">The configuration as an instance of the ECSClientConfiguration structure.</param>
        virtual void Initialize(const ECSClientConfiguration& config) = 0;

        /// <summary>
        /// Adds a listener to the ECS client - to be notified of configuration changes.
        /// </summary>
        /// <param name="listener">The listener to add to the ECS client.</param>
        /// <returns>A boolean value that indicates success (true) or failure (false).</returns>
        virtual bool AddListener(IECSClientCallback* listener) = 0;

        /// <summary>
        /// Removes the listener to stop receiving notifications from the ECS client.
        /// </summary>
        /// <param name="listener">The listener to remove from the ECSClient.</param>
        /// <returns>A boolean value that indicates success (true) or failure (false).</returns>
        virtual bool RemoveListener(IECSClientCallback* listener) = 0;

        /// <summary>
        /// Registers a logger to auto-tag events sent by the logger,
        /// taking a pointer to an ILogger interface, and a string that contains the agent name.
        /// </summary>
        /// <param name="pLoger">The logger to be registered with the ECS client</param>
        /// <param name="agentName">A string that contains the name of the agent.</param>
        /// <returns>A boolean value that indicates success (true) or failure (false).</returns>
        virtual bool RegisterLogger(Microsoft::Applications::Telemetry::ILogger* pLoger, const std::string& agentName) = 0;

        /// <summary>
        /// Sets a user ID used as the request parameter for retrieving configurations from the ECS server.
        /// The client can optionally pass this in the request so that the configuration can be 
        /// allocated on a per-user basic. The allocation persists for the user ID, 
        /// therefore it is good for the allocation to follow the user account.
        /// </summary>
        /// <param name="userId">A string that contains the login user ID to pass in the request.</param>
        /// <returns>A boolean value that indicates success (true) or failure (false).</returns>
        virtual bool SetUserId(const std::string& userId) = 0;

        /// <summary>
        /// Sets a device ID used as the request parameter for retrieving configurations from the ECS server.
        /// </summary>
        /// <param name="deviceId">A string that contains the device ID.</param>
        /// <returns>A boolean value that indicates success (true) or failure (false).</returns>
        virtual bool SetDeviceId(const std::string& deviceId) = 0;

        /// <summary>
        /// Sets a list of custom parameters for the request to use to retrieve configurations from ECS server
        /// </summary>
        /// <param name="requestParams"A map that contains the request parameters.</param>
        /// <returns>A boolean value that indicates success (true) or failure (false).</returns>
        virtual bool SetRequestParameters(const std::map<std::string, std::string>& requestParams) = 0;

        /// <summary>
        /// Starts the ECSClient - to retrieve configurations from the ECS server.
        /// </summary>
        /// <returns>A boolean value that indicates success (true) or failure (false).</returns>
        virtual bool Start() = 0;

        /// <summary>
        /// Stops the ECSClient from retrieving configurations from the ECS server.
        /// </summary>
        /// <returns>A boolean value that indicates success (true) or failure (false).</returns>
        virtual bool Stop() = 0;

        /// <summary>
        /// Suspends the ECSClient from retrieving configuration updates from the ECS server.
        /// </summary>
        /// <returns>A boolean value that indicates success (true) or failure (false).</returns>
        virtual bool Suspend() = 0;

        /// <summary>
        /// Resumes the ECSClient to retrieve configuration updates from the ECS server.
        /// </summary>
        /// <returns>A boolean value that indicates success (true) or failure (false).</returns>
        virtual bool Resume() = 0;

        /// <summary>
        /// Gets the ETag of the currently active ECS configuration.
        /// </summary>
        /// <returns>A string that contains the ETag.</returns>
        virtual std::string GetETag() = 0;

        /// <summary>
        /// Gets the ETag of the currently active ECS configuration.
        /// </summary>
        /// <returns>A string that contains the ETag.</returns>
        virtual std::string GetConfigs() = 0;

        /// <summary>
        /// Gets the keys under the specified configuration path, for the specified agent.
        /// </summary>
        /// <param name="agentName">A string that contains the name of the agent.</param>
        /// <param name="keysPath">A string that contains the configuration path.</param>
        /// <returns>list of configuration keys</returns>
        virtual std::vector<std::string> GetKeys(const std::string& agentName, const std::string& keysPath) = 0;

        /// <summary>
        /// Gets the setting for the specified agent, from the specified configuration path, taking a default string value.
        /// </summary>
        /// <param name="agentName">A string that contains the name of the agent.</param>
        /// <param name="settingPath">A string that contains the configuration path.</param>
        /// <param name="defaultValue">A string that contains the default value to return if no configuration setting can be found.</param>
        /// <returns>A string that contains the setting. The default value is returned if no configuration setting can be found.</returns>
        virtual std::string GetSetting(const std::string& agentName, const std::string& settingPath, const std::string& defaultValue) = 0;

        /// <summary>
        /// Gets the setting for the specified agent, from the specified configuration path, taking a default boolean value.
        /// </summary>
        /// <param name="agentName">A string that contains the name of the agent.</param>
        /// <param name="settingPath">A string that contains the configuration path.</param>
        /// <param name="defaultValue">A boolean that contains the default value to return if no configuration setting can be found.</param>
        /// <returns>A boolean value that contains the setting. The default value is returned if no configuration setting can be found.</returns>
        virtual bool GetSetting(const std::string& agentName, const std::string& settingPath, const bool defaultValue) = 0;

        /// <summary>
        /// Gets the setting for the specified agent, from the specified configuration path, taking a default integer value.
        /// </summary>
        /// <param name="agentName">A string that contains the name of the agent.</param>
        /// <param name="settingPath">A string that contains the configuration path.</param>
        /// <param name="defaultValue">An integer that contains the default value to return if no configuration setting can be found.</param>
        /// <returns>An integer that contains the setting. The default value is returned if no configuration setting can be found.</returns>
        virtual int GetSetting(const std::string& agentName, const std::string& settingPath, const int defaultValue) = 0;

        /// <summary>
        /// Gets the setting for the specified agent, from the specified configuration path, taking a default double value.
        /// </summary>
        /// <param name="agentName">A string that contains the name of the agent.</param>
        /// <param name="settingPath">A string that contains the configuration path.</param>
        /// <param name="defaultValue">A double that contains the default value to return if no configuration setting can be found.</param>
        /// <returns>A double that contains the setting. The default value is returned if no configuration setting can be found.</returns>
        virtual double GetSetting(const std::string& agentName, const std::string& settingPath, const double defaultValue) = 0;

        /// <summary>
        /// Gets a collection of settings for the specified agent, from the specified configuration path.
        /// </summary>
        /// <param name="agentName">A string that contains the name of the agent.</param>
        /// <param name="settingPath">A string that contains the configuration path.</param>
        /// <returns>A standard vector that contains the settings as strings.</returns>
        virtual std::vector<std::string> GetSettings(const std::string& agentName, const std::string& settingPath) = 0;

        /// <summary>
        /// Gets a collection of settings for the specified agent, from the specified configuration path.
        /// </summary>
        /// <param name="agentName">A string that contains the name of the agent.</param>
        /// <param name="settingPath">A string that contains the configuration path.</param>
        /// <returns>A vector that contains the settings as integers.</returns>
        virtual std::vector<int> GetSettingsAsInts(const std::string& agentName, const std::string& settingPath) = 0;

        /// <summary>
        /// Gets a collection of settings for the specified agent, from the specified configuration path.
        /// </summary>
        /// <param name="agentName">A string that contains the name of the agent.</param>
        /// <param name="settingPath">A string that contains the configuration path.</param>
        /// <returns>A vector that contains the settings as doubles.</returns>
        virtual std::vector<double> GetSettingsAsDbls(const std::string& agentName, const std::string& settingPath) = 0;
    };

}}}} // namespaces

