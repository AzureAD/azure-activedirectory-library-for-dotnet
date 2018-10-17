
#ifndef ARIA_LOGMANAGER_HPP
#define ARIA_LOGMANAGER_HPP

#include "ILogger.hpp"
#include "ILogConfiguration.hpp"
#ifdef _WIN32
#include "TransmitProfiles.hpp"
#include "DebugEvents.hpp"
#endif


#include <climits>
#include <cstdint>


namespace Microsoft {
    namespace Applications {
        namespace Telemetry {

            class LogSessionData;

            /// <summary>
            /// The LogManager class manages the Telemetry logging system.
            /// </summary>
            class ARIASDK_LIBABI LogManager
            {
            public:
                /// <summary>
                /// Initializes the telemetry logging system with the default configuration.
                /// </summary>
                /// <param name="tenantToken">The tenant token associated with the application.</param>
                /// <returns>A logger instance instantiated with the tenantToken.</returns>
                static ILogger* ARIASDK_LIBABI_CDECL Initialize(
#ifdef ANDROID
                    JNIEnv *env,
                    jclass contextClass,
                    jobject  contextObject,
#endif
                    const std::string& tenantToken);

                /// <summary>
                /// Flushes any pending telemetry events in memory to disk, and tears down the telemetry logging system.
                /// </summary>
                static void ARIASDK_LIBABI_CDECL FlushAndTeardown();

                /// <summary>
                /// Tries to send any pending telemetry events in memory or on disk.
                /// </summary>
                static void ARIASDK_LIBABI_CDECL UploadNow();

                /// <summary>
                /// Flushes any pending telemetry events in memory to disk to reduce possible data loss.
                /// This method can be expensive, so you should use it sparingly. The operating system blocks the calling thread, 
                /// and might flush the global file buffers (all buffered file system data) to disk, which can be
                /// time consuming.
                /// </summary>
                static void ARIASDK_LIBABI_CDECL Flush();

                /// <summary>
                /// Pauses the transmission of events to the data collector.
                /// While paused, events continue to be queued on the client (cached either in memory or on disk).
                /// </summary>
                static void ARIASDK_LIBABI_CDECL PauseTransmission();

                /// <summary>
                /// Resumes the transmission of events to the data collector.
                /// </summary>
                static void ARIASDK_LIBABI_CDECL ResumeTransmission();

                /// <summary>
                /// Sets the transmit profile to one of the built-in profiles.
                /// A transmit profile is a collection of hardware and system settings (like network connectivity, power state, etc.).
                /// </summary>
                /// <param name="profile">The transmit profile.</param>
                /// <returns>This function doesn't return any value because it always succeeds.</returns>
                static void ARIASDK_LIBABI_CDECL SetTransmitProfile(TransmitProfile profile);
#ifndef ANDROID
                /// <summary>
                /// Sets transmit profile for event transmission.
                /// A transmit profile is a collection of hardware and system settings (like network connectivity, power state)
                /// </summary>
                /// <param name="profile">A string that contains the transmit profile.</param>
                /// <returns>A boolean value that indicates success (true) or failure (false).</returns>
                static bool ARIASDK_LIBABI_CDECL SetTransmitProfile(const std::string& profile);

                /// <summary>
                /// Loads transmit profiles from a configuration in JSON.
                /// </summary>
                /// <param name="profiles_json">The configuration in JSON.</param>
                /// <returns>A boolean value that indicates success (true) or invalid configuration (false).</returns>
                static bool ARIASDK_LIBABI_CDECL LoadTransmitProfiles(std::string profiles_json);

                /// <summary>
                /// Resets transmission profiles to default settings.
                /// </summary>
                static void ARIASDK_LIBABI_CDECL ResetTransmitProfiles();

                /// <summary>Gets a transmit profile name based on one of the built-in transmit profiles.<summary>
                /// <param name="profile">One of the TransmitProfile enumeration values.</param>
                static const std::string ARIASDK_LIBABI_CDECL GetTransmitProfileName(TransmitProfile profile);
#endif
                /// <summary>
                /// Gets an ISemanticContext interface through which to specify context information 
                /// (such as device, system, hardware, and user information).
                /// Context information set using this API applies to all logger instance - unless they 
                /// are overwritten by an individual logger instance.
                /// </summary>
                /// <returns>A pointer to an ISemanticContext interface.</returns>
                static ISemanticContext* ARIASDK_LIBABI_CDECL GetSemanticContext();

                /// <summary>
                /// Adds (or overrides) a property to the custom context of the telemetry logging system,
                /// taking a string that contains the property name, 
                /// a string that contains the property value, 
                /// and the kind of customer content.
                /// Context information set here applies to events generated by all ILogger instances
                /// unless it is overwritten on a particular ILogger instance.
                /// </summary>
                /// <param name="name">A string that contains the name of the context property.</param>
                /// <param name="value">A string that contains the context property value.</param>
                /// <param name='ccKind'>One of the ::CustomerContentKind enumeration values.</param>
                static void ARIASDK_LIBABI_CDECL SetContext(const std::string& name, const std::string& value, CustomerContentKind ccKind);

                /// <summary>
                /// Adds (or overrides) a property to the custom context of the telemetry logging system,
                /// taking a string that contains the property name, 
                /// a character pointer to the property string value, 
                /// and the kind of customer content.
                /// Context information set here applies to events generated by all %ILogger instances
                /// unless it is overwritten on a particular %ILogger instance.
                /// </summary>
                /// <param name="name">A string that contains the name of the context property.</param>
                /// <param name="value">A character pointer to the property string value.</param>
                /// <param name='ccKind'>The kind of customer content.</param>
                static void ARIASDK_LIBABI_CDECL SetContext(const std::string& name, const char *value, CustomerContentKind ccKind);
                
                /// <summary>
                /// Adds (or overrides) a property to the custom context of the telemetry logging system,
                /// taking a string that contains the property name, 
                /// a string that contains the property value, 
                /// and the kind of personal identifiable information.
                /// Context information set here applies to events generated by all %ILogger instances
                /// unless it is overwritten on a particular %ILogger instance.
                /// </summary>
                /// <param name="name">A string that contains the name of the context property.</param>
                /// <param name="value">A string that contains the context property value.</param>
                /// <param name='piiKind'>One of the ::PiiKind enumeration values. The default value is <i>PiiKind_None</i>.</param>
                static void ARIASDK_LIBABI_CDECL SetContext(const std::string& name, const std::string& value, PiiKind piiKind = PiiKind_None);

                /// <summary>
                /// Adds (or overrides) a property to the custom context of the telemetry logging system,
                /// taking a string that contains the property name, 
                /// a constant character pointer to the property string value,
                /// and the kind of personal identifiable information.
                /// Context information set here applies to events generated by all %ILogger instances
                /// unless it is overwritten on a particular %ILogger instance.
                /// </summary>
                /// <param name="name">A string that contains the name of the context property.</param>
                /// <param name="value">A constant character pointer to a string that contains the context property value.</param>
                /// <param name='piiKind'>One of the ::PiiKind enumeration values. The default value is <i>PiiKind_None</i>.</param>
                static void ARIASDK_LIBABI_CDECL SetContext(const std::string& name, const char *value, PiiKind piiKind = PiiKind_None) { const std::string val(value); SetContext(name, val, piiKind); };

                /// <summary>
                /// Adds (or overrides) a property to the custom context of the telemetry logging system,
                /// taking a string that contains the property name, 
                /// a double that contains the property value,
                /// and the kind of personal identifiable information.
                /// Context information set here applies to events generated by all %ILogger instances
                /// unless it is overwritten on a particular %ILogger instance.
                /// </summary>
                /// <param name="name">A string that contains the name of the context property.</param>
                /// <param name="value">A double that contains the context property value.</param>
                /// <param name='piiKind'>One of the ::PiiKind enumeration values. The default value is <i>PiiKind_None</i>.</param>
                static void ARIASDK_LIBABI_CDECL SetContext(const std::string& name, double value, PiiKind piiKind = PiiKind_None);

                /// <summary>
                /// Adds (or overrides) a property to the custom context of the telemetry logging system,
                /// taking a string that contains the property name, 
                /// an int64_t that contains the property value,
                /// and the kind of personal identifiable information.
                /// Context information set here applies to events generated by all %ILogger instances
                /// unless it is overwritten on a particular %ILogger instance.
                /// </summary>
                /// <param name="name">A string that contains the name of the context property.</param>
                /// <param name="value">An int64_t that contains the context property value.</param>
                /// <param name='piiKind'>One of the ::PiiKind enumeration values. The default value is <i>PiiKind_None</i>.</param>
                static void ARIASDK_LIBABI_CDECL SetContext(const std::string& name, int64_t value, PiiKind piiKind = PiiKind_None);

                /// <summary>
                /// Adds (or overrides) a property to the custom context of the telemetry logging system,
                /// taking a string that contains the property name, 
                /// an int8_t that contains the property value,
                /// and the kind of personal identifiable information.
                /// Context information set here applies to events generated by all %ILogger instances
                /// unless it is overwritten on a particular %ILogger instance.<br>
                /// <b>Note:</b> All integer types other than <b>int64_t</b> are currently converted to <b>int64_t</b>.
                /// </summary>
                /// <param name="name">A string that contains the name of the context property.</param>
                /// <param name="value">An 8-bit integer property value.</param>
                /// <param name='piiKind'>One of the ::PiiKind enumeration values. The default value is <i>PiiKind_None</i>.</param>
                static inline void ARIASDK_LIBABI_CDECL SetContext(const std::string& name, int8_t  value, PiiKind piiKind = PiiKind_None) { SetContext(name, (int64_t)value, piiKind); }

                /// <summary>
                /// Adds (or overrides) a property to the custom context of the telemetry logging system,
                /// taking a string that contains the property name, 
                /// an int16_t that contains the property value,
                /// and the kind of personal identifiable information.
                /// Context information set here applies to events generated by all %ILogger instances
                /// unless it is overwritten on a particular %ILogger instance.<br>
                /// <b>Note:</b> All integer types other than <b>int64_t</b> are currently converted to <b>int64_t</b>.
                /// </summary>
                /// <param name="name">A string that contains the name of the context property.</param>
                /// <param name="value">A 16-bit integer property value.</param>
                /// <param name='piiKind'>One of the ::PiiKind enumeration values. The default value is <i>PiiKind_None</i>.</param>
                static inline void ARIASDK_LIBABI_CDECL SetContext(const std::string& name, int16_t value, PiiKind piiKind = PiiKind_None) { SetContext(name, (int64_t)value, piiKind); }

                /// <summary>
                /// Adds (or overrides) a property to the custom context of the telemetry logging system,
                /// taking a string that contains the property name, 
                /// an int32_t that contains the property value,
                /// and the kind of personal identifiable information.
                /// Context information set here applies to events generated by all %ILogger instances
                /// unless it is overwritten on a particular %ILogger instance.<br>
                /// <b>Note:</b> All integer types other than <b>int64_t</b> are currently converted to <b>int64_t</b>.
                /// </summary>
                /// <param name="name">A string that contains the name of the context property.</param>
                /// <param name="value">A 32-bit Integer property value.</param>
                /// <param name='piiKind'>One of the ::PiiKind enumeration values. The default value is <i>PiiKind_None</i>.</param>
                static inline void ARIASDK_LIBABI_CDECL SetContext(const std::string& name, int32_t value, PiiKind piiKind = PiiKind_None) { SetContext(name, (int64_t)value, piiKind); }

                /// <summary>
                /// Adds (or overrides) a property to the custom context of the telemetry logging system,
                /// taking a string that contains the property name, 
                /// a uint8_t that contains the property value,
                /// and the kind of personal identifiable information.
                /// Context information set here applies to events generated by all %ILogger instances
                /// unless it is overwritten on a particular %ILogger instance.<br>
                /// <b>Note:</b> All integer types other than <b>int64_t</b> are currently converted to <b>int64_t</b>.
                /// </summary>
                /// <param name="name">A string that contains the name of the context property.</param>
                /// <param name="value">An 8-bit unsigned integer property value.</param>
                /// <param name='piiKind'>One of the ::PiiKind enumeration values. The default value is <i>PiiKind_None</i>.</param>
                static inline void ARIASDK_LIBABI_CDECL SetContext(const std::string& name, uint8_t  value, PiiKind piiKind = PiiKind_None) { SetContext(name, (int64_t)value, piiKind); }

                /// <summary>
                /// Adds (or overrides) a property to the custom context of the telemetry logging system,
                /// taking a string that contains the property name, 
                /// a uint16_t that contains the property value,
                /// and the kind of personal identifiable information.
                /// Context information set here applies to events generated by all %ILogger instances
                /// unless it is overwritten on a particular %ILogger instance.<br>
                /// <b>Note:</b> All integer types other than <b>int64_t</b> are currently converted to <b>int64_t</b>.
                /// </summary>
                /// <param name="name">A string that contains the name of the context property.</param>
                /// <param name="value">A 16-bit unsigned integer property value.</param>
                /// <param name='piiKind'>One of the ::PiiKind enumeration values. The default value is <i>PiiKind_None</i>.</param>
                static inline void ARIASDK_LIBABI_CDECL SetContext(const std::string& name, uint16_t value, PiiKind piiKind = PiiKind_None) { SetContext(name, (int64_t)value, piiKind); }

                /// <summary>
                /// Adds (or overrides) a property to the custom context of the telemetry logging system,
                /// taking a string that contains the property name, 
                /// a uint32_t that contains the property value,
                /// and the kind of personal identifiable information.
                /// Context information set here applies to events generated by all %ILogger instances
                /// unless it is overwritten on a particular %ILogger instance.<br>
                /// <b>Note:</b> All integer types other than <b>int64_t</b> are currently converted to <b>int64_t</b>.
                /// </summary>
                /// <param name="name">A string that contains the name of the context property.</param>
                /// <param name="value">A 32-bit unsigned integer property value.</param>
                /// <param name='piiKind'>One of the ::PiiKind enumeration values. The default value is <i>PiiKind_None</i>.</param>
                static inline void ARIASDK_LIBABI_CDECL SetContext(const std::string& name, uint32_t value, PiiKind piiKind = PiiKind_None) { SetContext(name, (int64_t)value, piiKind); }

                /// <summary>
                /// Adds (or overrides) a property to the custom context of the telemetry logging system,
                /// taking a string that contains the property name, 
                /// a uint64_t that contains the property value,
                /// and the kind of personal identifiable information.
                /// Context information set here applies to events generated by all %ILogger instances
                /// unless it is overwritten on a particular %ILogger instance.<br>
                /// <b>Note:</b> All integer types other than <b>int64_t</b> are currently converted to <b>int64_t</b>.
                /// </summary>
                /// <param name="name">A string that contains the name of the context property.</param>
                /// <param name="value">A 64-bit unsigned integer property value.</param>
                /// <param name='piiKind'>One of the ::PiiKind enumeration values. The default value is <i>PiiKind_None</i>.</param>
                static inline void ARIASDK_LIBABI_CDECL SetContext(const std::string& name, uint64_t value, PiiKind piiKind = PiiKind_None) { SetContext(name, (int64_t)value, piiKind); }

                /// <summary>
                /// Adds (or overrides) a property to the custom context of the telemetry logging system,
                /// taking a string that contains the property name, 
                /// a boolean that contains the property value,
                /// and the kind of personal identifiable information.
                /// Context information set here applies to events generated by all %ILogger instances
                /// unless it is overwritten on a particular %ILogger instance.<br>
                /// <b>Note:</b> All integer types other than <b>int64_t</b> are currently converted to <b>int64_t</b>.
                /// </summary>
                /// <param name="name">A string that contains the name of the context property.</param>
                /// <param name="value">A .NET time_ticks_t property value.</param>
                /// <param name='piiKind'>One of the ::PiiKind enumeration values. The default value is <i>PiiKind_None</i>.</param>
                static void ARIASDK_LIBABI_CDECL SetContext(const std::string& name, bool value, PiiKind piiKind = PiiKind_None);

                /// <summary>
                /// Adds (or overrides) a property to the custom context of the telemetry logging system,
                /// taking a string that contains the property name, 
                /// a time_ticks_t that contains the property value,
                /// and the kind of personal identifiable information.
                /// Context information set here applies to events generated by all %ILogger instances
                /// unless it is overwritten on a particular %ILogger instance.<br>
                /// <b>Note:</b> All integer types other than <b>int64_t</b> are currently converted to <b>int64_t</b>.
                /// </summary>
                /// <param name="name">A string that contains the name of the context property.</param>
                /// <param name="value">A .NET time_ticks_t property value.</param>
                /// <param name='piiKind'>One of the ::PiiKind enumeration values. The default value is <i>PiiKind_None</i>.</param>
                static void ARIASDK_LIBABI_CDECL SetContext(const std::string& name, time_ticks_t value, PiiKind piiKind = PiiKind_None);

                /// <summary>
                /// Adds (or overrides) a property to the custom context of the telemetry logging system,
                /// taking a string that contains the property name, 
                /// a GUID_t that contains the property value,
                /// and the kind of personal identifiable information.
                /// Context information set here applies to events generated by all %ILogger instances
                /// unless it is overwritten on a particular %ILogger instance.<br>
                /// <b>Note:</b> All integer types other than <b>int64_t</b> are currently converted to <b>int64_t</b>.
                /// </summary>
                /// <param name="name">A string that contains the name of the context property.</param>
                /// <param name="value">A GUID_t property value.</param>
                /// <param name='piiKind'>One of the ::PiiKind enumeration values. The default value is <i>PiiKind_None</i>.</param>
                static void ARIASDK_LIBABI_CDECL SetContext(const std::string& name, GUID_t value, PiiKind piiKind = PiiKind_None);

                /// <summary>
                /// Gets the ILogger interface of a Logger instance through which to log telemetry events.
                /// </summary>
                /// <returns>A pointer to the Ilogger interface of a logger instance.</returns>
                static ILogger* ARIASDK_LIBABI_CDECL GetLogger();

                /// <summary>
                /// Gets the ILogger interface of a Logger instance through which to log telemetry events.
                /// </summary>
                /// <param name="source">A string that contains the name of the source of events.</param>
                /// <returns>Pointer to the Ilogger interface of the logger instance.</returns>
                static ILogger* ARIASDK_LIBABI_CDECL GetLogger(const std::string& source);

                /// <summary>
                /// Gets the ILogger interface of a Logger instance through which to log telemetry events.
                /// </summary>
                /// <param name="tenantToken">The tenant token associated with the application.</param>
                /// <param name="source">A string that contains the name of the source of events.</param>
                /// <returns>A pointer to the Ilogger interface of the logger instance.</returns>
                static ILogger* ARIASDK_LIBABI_CDECL GetLogger(const std::string& tenantToken, const std::string& source);

                /// <summary>
                /// Gets the log configuration.
                /// </summary>
                /// <returns>The log configuratioh as a reference to an ILogConfiguration.</returns>
                static ILogConfiguration& ARIASDK_LIBABI_CDECL GetLogConfiguration();
#ifdef _WIN32

                /// <summary>
                /// Adds a debug callback,
                /// taking a debug event type,
                /// and a specified debug event listener.
                /// </summary>
                /// <param name="type">One of the DebugEventType enumeration values.</param>
                /// <param name="listener">A reference to a DebugEventListener object.</param>
                static void ARIASDK_LIBABI_CDECL AddEventListener(DebugEventType type, DebugEventListener &listener);

                /// <summary>
                /// Removes a debug callback,
                /// taking a debug event type,
                /// and the specified debug event listener.
                /// </summary>
                /// <param name="type">One of the DebugEventType enumeration values.</param>
                /// <param name="listener">A reference to the DebugEventListener object that you want to remove.</param>
                static void ARIASDK_LIBABI_CDECL RemoveEventListener(DebugEventType type, DebugEventListener &listener);

/// @cond INTERNAL_DOCS
                /// <summary>
                /// Dispatches a debug event of the specified type.
                /// </summary>
                /// <param name="type">One of the DebugEventType enumeration types.</param>
                static bool ARIASDK_LIBABI_CDECL DispatchEvent(DebugEventType type);

                /// <summary>
                /// Dispatches the specified event to a client callback.
                /// </summary>
                /// <param name="evt">A reference to a DebugEvent object.</param>
                static bool ARIASDK_LIBABI_CDECL DispatchEvent(DebugEvent &evt);
/// @endcond

                /// <summary>
                /// Gets the log session data.
                /// </summary>
                /// <returns>The log session data in a pointer to a LogSessionData object.</returns>
                static LogSessionData* GetLogSessionData();

/// @cond INTERNAL_DOCS
                /// <summary>
                /// Sets a tenant-specific event exclusion filter
                /// </summary>
                /// <param name="tenantToken">Token of the tenant with which the application is associated for collecting telemetry</param>
                /// <param name="filterStrings">The events to exclude from uploads, specified as an array of strings.<br>
                /// Each string is an exact match, or a "StartsWith" syntax if the last character is '*'</param>
                /// <param name="filterCount">The number of strings in filterStrings</param>
                /// <returns>A positive value on success, a negative value on failure. Never returns 0.</returns>
                static int32_t SetExclusionFilter(const char* tenantToken, const char** filterStrings, uint32_t filterCount);
/// @endcond

#endif
#ifdef ANDROID
                static jclass  GetGlobalInternalMgrImpl();
#endif



            protected:

                /// <summary>
                /// The LogManager constructor.
                /// </summary>
                LogManager();

                /// <summary>
                /// The LogManager copy constructor.
                /// </summary>
                LogManager(const LogManager&);

                /// <summary>
                /// [not implemented] The LogManager assignment operator.
                /// </summary>
                LogManager& operator=(const LogManager&);

                /// <summary>
                /// The LogManager destructor.
                /// </summary>
                virtual ~LogManager() {};

                /// <summary>
                /// A debug routine that validates whether %LogManager has been initialized. Might trigger a warning message if %LogManager is not initialized.
                /// </summary>
                static void checkup();
            };
        }
    }
}
#endif //ARIA_LOGMANAGER_H