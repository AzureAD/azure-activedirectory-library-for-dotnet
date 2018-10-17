#pragma once
#include "ctmacros.hpp"
#include "ILogger.hpp"

#include <string>
#include <vector>
#include <map>

namespace Microsoft { namespace Applications { namespace Experimentation { namespace AFD {

    /// <summary>
    /// The AFDClientConfiguration structure configures the AFD client.
    /// </summary>
    struct AFDClientConfiguration
    {
        /// <summary>
        /// A string that contains the name of the client whose the AFD configurations are to be retrieved.<br>
        /// [required] Header X-MSEDGE-CLIENTID or parameter &clientid.
        /// </summary>
        std::string clientId;

		/// <summary>
		/// A string that contains the impression GUID.
		/// [optional] Header X-MSEDGE-IG  or parameter &ig=.
		/// </summary>
		std::string impressionGuid;

		/// <summary>
		/// A string that contains the name of the market.
		/// [optional] Header X-MSEDGE-MARKET  or parameter  &mkt
		/// </summary>
		std::string market;
			
        /// <summary>
        /// An integer that specifies whether the user is an existing user. <i>1</i> for yes, <i>0</i> for no.
        /// [optional] Header X-MSEDGE-EXISTINGUSER
        /// </summary> 
		int existingUser;

        /// <summary>
        /// An integer that specifies whether the session takes place over the corpnet. <i>1</i> for yes, <i>0</i> for no.
        /// [optional] parameter &corpnet=0
        /// </summary> 
        int corpnet = 1 ;

        /// <summary>
        ///A string that contains the name of the flight.
        /// [optional] parameter &setflight =
        /// </summary>
        std::string setflight;

        /// <summary>
        /// [required] A string that contains the version of the client.
        /// </summary>
        std::string clientVersion;

        /// <summary>
        /// [required] A string that contains the fully-qualified path name of the file used by the AFD client to cache the configuration details locally.
        /// </summary>
        std::string cacheFilePathName;

        /// <summary>
        /// [optional] An unsigned integer that contains the expiration time (in minutes) for the locally cached client configuration.
        /// </summary>
        unsigned int defaultExpiryTimeInMin = 0;

        /// <summary>
        /// [Required] A standard vector of strings that contains the AFD server URLs. If you don't specify any, then the default is used.
        /// </summary>
        std::vector<std::string> serverUrls;

		/// <summary>
        /// [optional] A boolean value that specifies whether verbose logging is used for debugging flights.
        /// If you don't specify this value, then the default value (<i>false</i>) is used.
        /// </summary>
        bool verbose = false;

        // [optional] enabled AFD telemetry
        bool enableAFDClientTelemetry = false;
    };

    /// <summary>
    /// The IAFDClientCallback class implements the Callback for the AFDClient to notify its listener of events that have occurred.
    /// </summary>
    class IAFDClientCallback
    {
    public:
       
		/// <summary>
        /// The AFDClientEventType enumeration contains a set of values that specify the result of a configuration update.
        /// </summary>
		enum AFDClientEventType
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
        /// The AFDClientEventContext structure contains the state of the AFD client event context.
        /// </summary>
        struct AFDClientEventContext
        {
			/// <summary>
            /// A string that contains the client ID.
            /// </summary>
            std::string clientId;

			/// <summary>
            /// A string that contains the client version.
            /// </summary>
            std::string clientVersion;

			/// <summary>
            /// A string that contains the impression ID (the identifier of the currently running flights).
            /// </summary>
			std::string impressionID;

			/// <summary>
            /// An int64-t that specifies the flighting version.
            /// </summary>
			std::int64_t flightingVersion;

			/// <summary>
            /// A standard map that contains the request headers.
            /// </summary>
            std::map<std::string, std::string> requestHeaders;

			/// <summary>
            /// A standard map that contains the request parameters.
            /// </summary>
            std::map<std::string, std::string> requestParameters;

			/// <summary>
            /// A standard vector that contains the list of features.
            /// </summary>
			std::vector<std::string> features;

			/// <summary>
            /// A standard vector that contains the names of the flights.
            /// </summary>
			std::vector<std::string> flights;
            std::map<std::string, std::string> configs;

			/// <summary>
            /// An unsigned integer that specifies the length of time (in seconds) that the configuration remains valid.
            /// </summary>
            unsigned int configExpiryTimeInSec;

			/// <summary>
            /// A boolean value that indicates that there is an update from the Azure Front Door server.
            /// </summary>
            bool configUpdateFromAFD;
        };

        /// <summary>
        /// The OnAFDClientEvent pure virtual function is a callback that allows the AFDClient to 
        /// notify its listener of configuration changes.
        /// </summary>
        /// <param name="evtType">The type of AFDClient event.</param>
        /// <param name="evtContext">The AFDClient event context information.</param>
        virtual void OnAFDClientEvent(AFDClientEventType evtType, AFDClientEventContext evtContext) = 0;
    };

    /// <summary>
    /// The IAFDClient class represents the interface to the IAFD client.
    /// </summary>
    class ARIASDK_LIBABI IAFDClient
    {
    public:
        /// <summary>
        /// Creates a new instance of an AFDClient.
        /// </summary>
        static IAFDClient* ARIASDK_LIBABI_CDECL CreateInstance();

        /// <summary>
        /// Destroys the specified AFDClient instance.
        /// </summary>
        static void ARIASDK_LIBABI_CDECL DestroyInstance(IAFDClient** ppAFDClient);

        /// <summary>
        /// Initializes an AFD client with the specified configuration.
        /// </summary>
        /// <param name="config">The AFD client configuration.</param>
		virtual void Initialize(const AFDClientConfiguration& config) = 0;

        /// <summary>
        /// Adds a listener.
        /// </summary>
         /// <param name="listener">A pointer to the listener.</param>
		virtual bool AddListener(IAFDClientCallback* listener) = 0;

        /// <summary>
        /// Removes a listener.
        /// </summary>
        /// <param name="listener">A pointer to an IAFDClientCallback listener.</param>
        /// <returns>A boolean value that indicates success (true) or failure (false).</returns>
		virtual bool RemoveListener(IAFDClientCallback* listener) = 0;
		
		/// <summary>
		/// Registers a logger to auto-tag events sent by the logger with AFD configuration information (like Etag).
		/// </summary>
		/// <param name="pLoger">The logger to register with the AFD client.</param>
		/// <param name="agentName">A string that contains the name of the agent whose experiment configIds will be 
		/// auto-tagged to events sent by the logger.</param>
		/// <returns>A boolean value that indicates success (true) or failure (false).</returns>
		virtual bool RegisterLogger(Microsoft::Applications::Telemetry::ILogger* pLoger, const std::string& agentName) = 0;

		/// <summary>
		/// Sets a list of custom parameters for the request to use to retrieve configurations from the AFD server.
		/// </summary>
		/// <param name="requestParams">A list of parameters for the request.</param>
		/// <returns>A boolean value that indicates success (true) or failure (false).</returns>
		virtual bool SetRequestParameters(const std::map<std::string, std::string>& requestParams) = 0;

        /// <summary>
		/// Sets a list of request headers for the request to use to retrieve configurations from the AFD server.
		/// </summary>
		/// <param name="headerParams">A standard map that contains headers for the request.</param>
		/// <returns>A boolean value that indicates success (true) or failure (false).</returns>
		virtual bool SetRequestHeaders(const std::map<std::string, std::string>& headerParams) = 0;

		/// <summary>
		/// Starts the AFDClient to retrieve configurations from the AFD server.
		/// </summary>
		/// <returns>A boolean value that indicates success (true) or failure (false).</returns>
		virtual bool Start() = 0;

		/// <summary>
		/// Prevents the AFDClient from retrieving configurations from the AFD server.
		/// </summary>
		/// <returns>A boolean value that indicates success (true) or failure (false).</returns>
		virtual bool Stop() = 0;

		/// <summary>
		/// Suspends the AFDClient to prevent it from retrieving configuration updates from the AFD server.
		/// </summary>
		/// <returns>A boolean value that indicates success (true) or failure (false).</returns>
		virtual bool Suspend() = 0;

		/// <summary>
		/// Resumes the AFDClient so it continues to retrieve configuration updates from the AFD server.
		/// </summary>
		/// <returns>A boolean value that indicates success (true) or failure (false).</returns>
		virtual bool Resume( bool fetchConfig = true) = 0;

		/// <summary>
		/// Gets the flights of the currently active AFD configuration.
		/// </summary>
		/// <returns>A vector that contains strings that contain the names of the flights.</returns>
		virtual std::vector<std::string> GetFlights() = 0;

		/// <summary>
		/// Gets the features of the currently active AFD configuration.
		/// </summary>
		/// <returns>A standard vector that contains strings that contain the names of the features.</returns>
		virtual std::vector<std::string> GetFeatures() = 0;

        /// <summary>
        /// get the configs of the current active AFD configuration
        /// </summary>
        /// <returns>vector of feature string</returns>
        virtual std::map<std::string, std::string> GetConfigs() = 0;

		/// <summary>
		/// Gets the ETag of the currently active AFD configuration.
		/// </summary>
		/// <returns>A string that contains the ETag.</returns>
		virtual std::string GetETag() = 0;

		/// <summary>
		/// Gets the keys under the specified configuration path of the specified agent.
		/// </summary>
		/// <param name="agentName">A string that contains the name of the agent.</param>
		/// <param name="keysPath">A string that contains the configuration path.</param>
		/// <returns>list of configuration keys</returns>
		virtual std::vector<std::string> GetKeys(const std::string& agentName, const std::string& keysPath) = 0;

		/// <summary>
		/// Gets the IAFDClient setting, using the specified agent name, setting path, and default string value.
		/// </summary>
		/// <param name="agentName">A string that contains the name of the agent.</param>
		/// <param name="settingPath">A string that contains the configuration path.</param>
		/// <param name="defaultValue">A string that contains the default value to return if no configuration setting is found.</param>
		/// <returns>A string that contains the setting. This method returns the specified default if no configuration setting is found.</returns>
		virtual std::string GetSetting(const std::string& agentName, const std::string& settingPath, const std::string& defaultValue) = 0;

		/// <summary>
		/// Gets the IAFDClient setting, using the specified agent name, setting path, and default boolean value.
		/// </summary>
		/// <param name="agentName">A string that contains the name of the agent.</param>
		/// <param name="settingPath">A string that contains the configuration path.</param>
		/// <param name="defaultValue">A boolean that contains the default value to return if no configuration setting is found.</param>
		/// <returns>A boolean that contains the setting. This method returns the specified default if no configuration setting is found.</returns>
		virtual bool GetSetting(const std::string& agentName, const std::string& settingPath, const bool defaultValue) = 0;

		/// <summary>
		/// Gets the IAFDClient setting, using the specified agent name, setting path, and default integer value.
		/// </summary>
		/// <param name="agentName">A string that contains the name of the agent.</param>
		/// <param name="settingPath">A string that contains the configuration path.</param>
		/// <param name="defaultValue">An integer that contains the default value to return if no configuration setting is found.</param>
		/// <returns>An integer that contains the setting. This method returns the specified default if no configuration setting is found.</returns>
		virtual int GetSetting(const std::string& agentName, const std::string& settingPath, const int defaultValue) = 0;

		/// <summary>
		/// Gets the IAFDClient setting, using the specified agent name, setting path, and default double value.
		/// </summary>
		/// <param name="agentName">A string that contains the name of the agent.</param>
		/// <param name="settingPath">A string that contains the configuration path.</param>
		/// <param name="defaultValue">A double that contains the default value to return if no configuration setting is found.</param>
		/// <returns>A double that contains the setting. This method returns the specified default if no configuration setting is found.</returns>
		virtual double GetSetting(const std::string& agentName, const std::string& settingPath, const double defaultValue) = 0;

		/// <summary>
		/// Gets the IAFDClient setting, using the specified agent name and setting path.
		/// </summary>
		/// <param name="agentName">A string that contains the name of the agent.</param>
		/// <param name="settingPath">A string that contains the configuration path.</param>
		/// <returns>A standard vector that contains strings that contain the list of settings.</returns>
		virtual std::vector<std::string> GetSettings(const std::string& agentName, const std::string& settingPath) = 0;

		/// <summary>
		/// Gets the IAFDClient settings and integers, using the specified agent name and setting path.
		/// </summary>
		/// <param name="agentName">A string that contains the name of the agent.</param>
		/// <param name="settingPath">A string that contains the configuration path.</param>
		/// <returns>A standard vector that contains integers that contain the list of settings.</returns>
		virtual std::vector<int> GetSettingsAsInts(const std::string& agentName, const std::string& settingPath) = 0;

		/// <summary>
		/// Gets the IAFDClient settings and doubles, using the specified agent name and setting path.
		/// </summary>
		/// <param name="agentName">A string that contains the name of the agent.</param>
		/// <param name="settingPath">A string that contains the configuration path.</param>
		/// <returns>A standard vector that contains doubles that contain the list of settings.</returns>
		virtual std::vector<double> GetSettingsAsDbls(const std::string& agentName, const std::string& settingPath) = 0;

        /// <summary>
        /// Gets the IAFDClient configuration json as string.
        /// </summary>
        /// <returns>A string that contains the ETag.</returns>
        virtual std::string GetAFDConfiguration() = 0; 
        
    };
   
}}}} // namespaces

