#ifndef ARIA_ILOGCONFIGURATION_HPP
#define ARIA_ILOGCONFIGURATION_HPP

#include "Version.hpp"
#include "Enums.hpp"
#include "ctmacros.hpp"
#include "stdint.h"

// *INDENT-OFF*
namespace Microsoft {
    namespace Applications {
        namespace Telemetry {
            // *INDENT-ON*
            class IHttpClient;
            class IRuntimeConfig;
            class IBandwidthController;

            /// <summary>
            /// The URI of the United States collector.
            /// </summary>
            static const char* COLLECTOR_URL_UNITED_STATES = "https://us.pipe.aria.microsoft.com/Collector/3.0/";

            /// <summary>
            /// The URI of the German collector.
            /// </summary>
            static const char* COLLECTOR_URL_GERMANY = "https://de.pipe.aria.microsoft.com/Collector/3.0/";

            /// <summary>
            /// The URI of the Australian collector.
            /// </summary>
            static const char* COLLECTOR_URL_AUSTRALIA = "https://au.pipe.aria.microsoft.com/Collector/3.0/";

            /// <summary>
            /// The URI of the Japanese collector.
            /// </summary>
            static const char* COLLECTOR_URL_JAPAN = "https://jp.pipe.aria.microsoft.com/Collector/3.0/";

            /// <summary>
            /// The URI of the European collector.
            /// </summary>
            static const char* COLLECTOR_URL_EUROPE = "https://eu.pipe.aria.microsoft.com/Collector/3.0/";

            /// <summary>
            /// The real-time transmit profile.
            /// </summary>
            static constexpr const char* const TRANSMITPROFILE_REALTIME = "REAL_TIME";

            /// <summary>
            /// The near real-time transmit profile.
            /// </summary>
            static constexpr const char* const TRANSMITPROFILE_NEARREALTIME = "NEAR_REAL_TIME";

            /// <summary>
            /// The best effort transmit profile.
            /// </summary>
            static constexpr const char* const TRANSMITPROFILE_BESTEFFORT = "BEST_EFFORT";

            /// <summary>
            /// Enable analytics.
            /// </summary>
            static constexpr const char* const CFG_BOOL_ENABLE_ANALYTICS = "enableLifecycleSession";

            /// <summary>
            /// Enable multitenant.
            /// </summary>
            static constexpr const char* const CFG_BOOL_ENABLE_MULTITENANT = "multiTenantEnabled";

            /// <summary>
            /// Enable CRC-32 check.
            /// </summary>
            static constexpr const char* const CFG_BOOL_ENABLE_CRC32 = "enableCRC32";

            /// <summary>
            /// Enable HMAC authentication.
            /// </summary>
            static constexpr const char* const CFG_BOOL_ENABLE_HMAC = "enableHMAC";

            /// <summary>
            /// Enable database compression.
            /// </summary>
            static constexpr const char* const CFG_BOOL_ENABLE_DB_COMPRESS = "enableDBCompression";

            /// <summary>
            /// Enable WAL journal.
            /// </summary>
            static constexpr const char* const CFG_BOOL_ENABLE_WAL_JOURNAL = "enableWALJournal";

            /// <summary>
            /// The event collection URI.
            /// </summary>
            static constexpr const char* const CFG_STR_COLLECTOR_URL = "eventCollectorUri";

            /// <summary>
            /// The cache file-path.
            /// </summary>
            static constexpr const char* const CFG_STR_CACHE_FILE_PATH = "cacheFilePath";

            /// <summary>
            /// the cache file size limit in bytes.
            /// </summary>
            static constexpr const char* const CFG_INT_CACHE_FILE_SIZE = "cacheFileSizeLimitInBytes";

            /// <summary>
            /// The RAM queue size limit in bytes.
            /// </summary>
            static constexpr const char* const CFG_INT_RAM_QUEUE_SIZE = "cacheMemorySizeLimitInBytes";

            /// <summary>
            /// The size of the RAM queue buffers, in bytes.
            /// </summary>
            static constexpr const char* const CFG_INT_RAM_QUEUE_BUFFERS = "maxDBFlushQueues";

            /// <summary>
            /// The trace level mask.
            /// </summary>
            static constexpr const char* const CFG_INT_TRACE_LEVEL_MASK = "traceLevelMask";

            /// <summary>
            /// The minimum trace level.
            /// </summary>
            static constexpr const char* const CFG_INT_TRACE_LEVEL_MIN = "minimumTraceLevel";

            /// <summary>
            /// The SDK mode.
            /// </summary>
            static constexpr const char* const CFG_INT_SDK_MODE = "sdkmode";

            /// <summary>
            /// The maximum teardown time.
            /// </summary>
            static constexpr const char* const CFG_INT_MAX_TEARDOWN_TIME = "maxTeardownUploadTimeInSec";
            
            /// <summary>
            /// The maximum number of pending HTTP requests.
            /// </summary>
            static constexpr const char* const CFG_INT_MAX_PENDING_REQ = "maxPendingHTTPRequests";

            /// <summary>
            /// The maximum package drop on full.
            /// </summary>
            static constexpr const char* const CFG_INT_MAX_PKG_DROP_ON_FULL = "maxPkgDropOnFull";

            /// <summary>
            /// The cache file percentage full notification.
            /// </summary>
            static constexpr const char* const CFG_INT_CACHE_FILE_FULL_NOTIFICATION_PERCENTAGE = "cacheFileFullNotificationPercentage";

            /// <summary>
            /// The cache memory percentage full notification.
            /// </summary>
            static constexpr const char* const CFG_INT_CACHE_MEMORY_FULL_NOTIFICATION_PERCENTAGE = "cacheMemoryFullNotificationPercentage";

            /// <summary>
            /// PRAGMA journal mode.
            /// </summary>
            static constexpr const char* const CFG_STR_PRAGMA_JOURNAL_MODE = "PRAGMA_journal_mode";

            /// <summary>
            /// PRAGMA synchronous.
            /// </summary>
            static constexpr const char* const CFG_STR_PRAGMA_SYNCHRONOUS = "PRAGMA_synchronous";

            /// <summary>
            /// The ILogConfiguration is the interface for configuring the telemetry logging system.
            /// </summary>
            class ARIASDK_LIBABI ILogConfiguration
            {
            public:
                //virtual ~ILogConfguration() {}

                /// <summary>
                /// [optional] Sets the minimum debug trace level mask, which controls the global verbosity level.
                /// The default value is <i>ACTTraceLevel_Error</i>.
                /// </summary>
                /// <param name="minimumTraceLevel">The minimum trace level as one of the ACTTraceLevel enumeration values.</param>
                virtual void SetMinimumTraceLevel(ACTTraceLevel minimumTraceLevel) = 0;

                /// <summary>
                /// [optional] Gets the debug trace level mask, which controls the global verbosity level.
                /// The default value is <i>ACTTraceLevel_Error</i>.
                /// </summary>
                /// <returns>The minimum debug trace level as one of the ACTTraceLevel enumeration values.</returns>
                virtual ACTTraceLevel GetMinimumTraceLevel() const = 0;

                /// <summary>
                /// Sets the Aria SDK mode with either <i>Non UTC</i>, 
                /// <i>UTC with common Schema</i>, 
                /// or <i>UTC with Aria Schema</i>.
                /// The default value is <i>Non UTC</i>.
                /// </summary>
                /// <param name="sdkmode">The SDK mode as one of the SdkModeTypes enumeration values.</param>
                virtual void SetSdkModeType(SdkModeTypes sdkmode) = 0;

                /// <summary>
                /// Gets the Aria SDK mode (either <i>Non UTC</i>, 
                /// <i>UTC with common Schema</i>, 
                /// or <i>UTC with Aria Schema</i>).
                /// The default value is <i>Non UTC</i>.
                /// </summary>
                /// <returns>The SDK mode as one of the SdkModeTypes enumeration values.</returns>
                virtual SdkModeTypes GetSdkModeType() const = 0;

                /// <summary>
                /// Sets an ILogConfiguration property, taking two strings for a key/value pair.
                /// </summary>
                /// <param name="key">A pointer to a character constant that contains the key string.</param>
                /// <param name="value">A pointer to a character constant that contains the value string.</param>
                virtual void SetProperty(char const* key, char const* value) = 0;

                /// <summary>
                /// Sets an ILogConfiguration property, taking a string for the key, and a uint32_t for the value.
                /// </summary>
                /// <param name="key">A pointer to a character constant that contains the key string.</param>
                /// <param name="value">A uint32_t that contains the value.</param>
                virtual void SetIntProperty(char const* key, uint32_t value) = 0;

                /// <summary>
                /// Sets an ILogConfiguration property, taking a string for the key, and a boolean for the value.
                /// </summary>
                /// <param name="key">A pointer to a character constant that contains the key string.</param>
                /// <param name="value">A boolean that contains the value.</param>
                virtual void SetBoolProperty(char const* key, bool value) = 0;

                /// <summary>
                /// Sets an ILogConfiguration property, taking a string for the name, and a void pointer for the value.
                /// </summary>
                /// <param name="key">A pointer to a character constant that contains the key string.</param>
                /// <param name="value">A void pointer that points to the value.</param>
                virtual void SetPointerProperty(char const* key, void* value) = 0;

                /// <summary>
                /// Gets an ILogConfiguration property, taking a string for the key, and a boolean for the error.
                /// </summary>
                /// <param name="key">A pointer to a character constant that contains the key string.</param>
                /// <param name="error">A reference to a boolean that contains the error</param>
                /// <returns>The property, pointed to by a character constant pointer.</returns>
                virtual char const* GetProperty(char const* key, bool& error) const = 0;

                /// <summary>
                /// Gets an ILogConfiguration property, taking a string for the key, and a boolean for the error.
                /// </summary>
                /// <param name="key">A pointer to a character constant that contains the key string.</param>
                /// <param name="error">A reference to a boolean that contains the error.</param>
                /// <returns>The property in a uint32_t.</returns>
                virtual uint32_t GetIntProperty(char const* key, bool& error) const = 0;

                /// <summary>
                /// Gets an ILogConfiguration property, taking a string for the key, and a boolean for the error.
                /// </summary>
                /// <param name="key">A pointer to a character constant that contains the key string.</param>
                /// <param name="error">A reference to a boolean that contains the error.</param>
                /// <returns>The property in a boolean.</returns>
                virtual bool GetBoolProperty(char const* key, bool& error) const = 0;

                /// <summary>
                /// Gets an ILogConfiguration property,  API allows to get Aria pointer setting
                /// </summary>
                /// <param name="key">A pointer to a character constant that contains the key string.</param>
                /// <param name="error">A reference to a boolean that contains the error.</param>
                /// <returns>The property pointed to by a void pointer.</returns>
                virtual void* GetPointerProperty(char const* key, bool& error) const = 0;
            };
        }
    }
} // namespace Microsoft::Applications::Telemetry
#endif 