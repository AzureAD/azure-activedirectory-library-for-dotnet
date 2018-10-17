#pragma once
#include "Version.hpp"
#include <Enums.hpp>
#include <cstdint>
#include <vector>
#include <string>
#include <map>
#include <algorithm>

//#include <json/json.h>

/// @cond INTERNAL_DOCS

namespace ARIASDK_NS_BEGIN {

            /// <summary>
            /// The maximum number of transmit profiles.
            /// </summary>
            static const size_t MAX_TRANSMIT_PROFILES   = 20;

            /// <summary>
            /// The maximum number of transmit rules.
            /// </summary>
            static const size_t MAX_TRANSMIT_RULES      = 16;

            /// <summary>
            /// The maximum timer size.
            /// <b>Note:</b> The size must match <i>EventPriority_MAX-EventPriority_MIN+1</i> in ILogger.hpp.
            /// </summary>
            static const size_t MAX_TIMERS_SIZE         = 3;

            /// <summary>
            /// The TransmitProfileRule structure contains transmission timer values in particular device states (net+power).
            /// </summary>
            typedef struct TransmitProfileRule {

                /// <summary>
                /// The network cost, as one of the Microsoft::Applications::Telemetry::NetworkCost enumeration values.
                /// </summary>
                NetworkCost      netCost;         // any|unknown|low|high|restricted
                
                /// <summary>
                /// The power state, as one of the Microsoft::Applications::Telemetry::PowerSource enumeration values.
                /// </summary>
                PowerSource      powerState;      // any|unknown|battery|charging
                
                /// <summary>
                /// The type of network, as one of the Microsoft::Applications::Telemetry::NetworkType enumeration values.
                /// <b>Note:</b> This member is reserved for future use.
                /// </summary>
                NetworkType      netType;         // reserved for future use
                
                /// <summary>
                /// The speed of the network.
                /// <b>Note:</b> This member is reserved for future use.
                /// </summary>
                unsigned         netSpeed;        // reserved for future use
                
                /// <summary>
                /// A vector on integers that contain per-priority transmission timers.
                /// </summary>
                std::vector<int> timers;          // per-priority transmission timers

                /// <summary>
                /// The TransmitProfileRule structure default constructor.
                /// </summary>
                TransmitProfileRule() {
                    netCost    = NetworkCost_Any;
                    netType    = NetworkType_Any;
                    netSpeed   = 0;
                    powerState = PowerSource_Any;
                    timers.clear();
                }

            } TransmitProfileRule;

            /// <summary>
            /// A named profile that aggregates a set of transmission rules.
            /// </summary>
            typedef struct TransmitProfileRules {
                
                /// <summary>
                /// A string that contains the profile name.
                /// </summary>
                std::string name;                       // Profile name
                
                /// <summary>
                /// A vector that contains a set of transmit profile rules.
                /// </summary>
                std::vector<TransmitProfileRule> rules; // Transmit profile rules
            } TransmitProfileRules;

            /// <summary>
            /// The TransmitProfiles class manages transmit profiles.
            /// </summary>
            class TransmitProfiles {

            protected:
                /// <summary>
                /// A map that contains all transmit profiles.
                /// </summary>
                static std::map<std::string, TransmitProfileRules> profiles;

                /// <summary>
                /// A string that contains the name of the currently active transmit profile.
                /// </summary>
                static std::string      currProfileName;

                /// <summary>
                /// The size of the currently active transmit profile rule.
                /// </summary>
                static size_t           currRule;

                /// <summary>
                /// The last reported network cost, as one of the Microsoft::Applications::Telemetry::NetworkCost enumeration values.
                /// </summary>
                static NetworkCost      currNetCost;

                /// <summary>
                /// The last reported power state, as one of the Microsoft::Applications::Telemetry::PowerSource enumeration values.
                /// </summary>
                static PowerSource      currPowState;

                /// <summary>
                /// A boolean value that indicates whether the timer was updated.
                /// </summary>
                static bool         isTimerUpdated;

            public:

                
                /// <summary>
                /// The TransmitProfiles default constructor.
                /// </summary>
                TransmitProfiles();

                /// <summary>
                /// The TransmitProfiles destructor.
                /// </summary>
                virtual ~TransmitProfiles();

                /// <summary>
                /// Prints transmit profiles to the debug log.
                /// </summary>
                static void dump();

                /// <summary>
                /// Performs timer sanity check and auto-fixes timers if needed.
                /// <b>Note:</b> This function is not thread safe.
                /// </summary>
                /// <param name="rule">The transmit profile rule that contains the timers to adjust.</param>
                /// <returns>A boolean value that indicates success (true) or failure (false).</returns>
                static bool adjustTimers(TransmitProfileRule & rule);

                /// <summary>
                /// Removes custom profiles.
                /// This method is called from parse only, and does not require the lock.
                /// <b>Note:</b> This function is not thread safe.
                /// </summary>
                static void removeCustomProfiles();

                /// <summary>
                /// Parses transmit profiles from JSON.
                /// </summary>
                /// <param name="profiles_json">A string that contains the the transmit profiles in JSON.</param>
                /// <returns>The size (in bytes) of the resulting TransmitProfiles object.</returns>
                static size_t parse(const std::string& profiles_json);

                /// <summary>
                /// Loads customer-supplied transmit profiles.
                /// </summary>
                /// <param name="profiles_json">A string that contains the the transmit profiles in JSON.</param>
                /// <returns>A boolean value that indicates success (true) or failure (false).</returns>
                static bool load(std::string profiles_json);

                /// <summary>
                /// Resets transmit profiles to default values.
                /// </summary>
                static void reset();

				/// <summary>
				/// Sets the default transmit profile.
				/// </summary>
				/// <param name="profileName">The transmit profile to set as the default.</param>
				/// <returns>A boolean value that indicates success (true) or failure (false).</returns>
				static bool setDefaultProfile(const TransmitProfile profileName);

                /// <summary>
                /// Sets the active transmit profile.
                /// </summary>
                /// <param name="profileName">A string that contains the name of the transmit profile to set.</param>
                /// <returns></returns>
                static bool setProfile(const std::string& profileName);

                /// <summary>
                /// Gets the current priority timers.
                /// </summary>
                /// <param name="out">A reference to a vector of integers that will contain the current timers.</param>
                static void TransmitProfiles::getTimers(std::vector<int>& out);

                /// <summary>
                /// Gets the name of the current transmit profile.
                /// </summary>
                /// <returns>A string that contains the name of the current transmit profile.</returns>
                static std::string& getProfile();

                /// <summary>
                /// Gets the current device's network cost and power state.
                /// </summary>
                /// <param name="netCost">A reference to an instance of a Microsoft::Applications::Telemetry::NetworkCost enumeration.</param>
                /// <param name="powState">A reference to an instance of a Microsoft::Applications::Telemetry::PowerSource enumeration.</param>
                static void getDeviceState(NetworkCost &netCost, PowerSource &powState);

                /// <summary>
                /// A timer update event handler.
                /// </summary>
                static void onTimersUpdated();

                /// <summary>
                /// Determines whether a timer should be updated.
                /// </summary>
                /// <returns>A boolean value that indicates yes (true) or no (false).</returns>
                static bool isTimerUpdateRequired();                

                /// <summary>
                /// Selects a transmit profile rule based on the current device state.
                /// </summary>
                /// <param name="netCost">The network cost, as one of the 
                /// Microsoft::Applications::Telemetry::NetworkCost enumeration values.</param>
                /// <param name="powState">The power state, as one of the 
                /// Microsoft::Applications::Telemetry::PowerSource enumeration values.</param>
                /// <returns>A boolean value that indicates success (true) or failure (false).</returns>
                static bool updateStates(NetworkCost netCost, PowerSource powState);

            };

   } ARIASDK_NS_END

/// @endcond
