
#ifndef ISEMANTICCONTEXT_HPP
#define ISEMANTICCONTEXT_HPP

#include "ctmacros.hpp"
#include "Enums.hpp"
#include <string>

// *INDENT-OFF*
namespace Microsoft { namespace Applications { namespace Telemetry {
// *INDENT-ON*

/// <summary>
/// The ISemanticContext class represents a semantic context.
/// </summary>
class  ARIASDK_LIBABI ISemanticContext
{
  public:
    /// <summary>
    /// The ISemanticContext destructor.
    /// </summary>
    virtual  ~ISemanticContext() {}

    /// <summary>
    /// Sets the application identifier context information for a telemetry event.
    /// </summary>
    /// <param name="appId">A string that uniquely identifies the user-facing application from which this event originated.</param>
    virtual void  SetAppId(std::string const& appId) = 0;

    /// <summary>
    /// Sets the application version context information.
    /// </summary>
    /// <param name="appVersion">A string that contains the version of the application, retrieved programmatically where possible.</param>
    virtual void  SetAppVersion(std::string const& appVersion) = 0;

    /// <summary>
    /// Sets the application language context information (the name of the spoken language used in the application).
    /// </summary>
    /// <param name="appLanguage">A string that contains the name of the language.</param>
    virtual void  SetAppLanguage(std::string const& appLanguage) = 0;

	/// <summary>
	/// Sets the experiment IDs for the application.
	/// Experiment IDs are applied to all events - unless overwritten by SetEventExperimentIds().
	/// </summary>
	/// <param name="appExperimentIds">A string that contains the comma-separated list of experiment IDs.</param>
    virtual void  SetAppExperimentIds(std::string const& appExperimentIds) = 0;

	/// <summary>
	/// Sets the experiment tag (experiment configuration) context information for telemetry events.
	/// <b>Note:</b> This method removes any previously stored experiment IDs that were set using SetAppExperimentIds().
	/// </summary>
	/// <param name="appExperimentETag">A string that contains the ETag&mdash;which is a hash of the set of experiments.</param>
	virtual void  SetAppExperimentETag(std::string const& appExperimentETag) = 0;

	/// <summary>
	/// Sets the application experimentation impression ID context information for telemetry events.
	/// </summary>
	/// <param name="appExperimentImpressionId">A string that contains the list of impression IDs&mdash;which is a hash of flights..</param>
	virtual void  SetAppExperimentImpressionId(std::string const& appExperimentImpressionId) = 0;
	
	/// <summary>
	/// Sets the experiment IDs for the specified telemetry event.
	/// </summary>
	/// <param name="eventName">A string that contains the event name.</param>
    /// <param name="experimentIds">A string that contains the list of IDs of experiments into which the application is enlisted.</param>
	virtual void  SetEventExperimentIds(std::string const& eventName, std::string const& experimentIds) = 0;

    /// <summary>
    /// Sets the device identifier context information for telemetry events.
    /// </summary>
    /// <param name="deviceId">A string that contains the a unique device identifier, retrieved programmatically.</param>
    virtual void  SetDeviceId(std::string const& deviceId) = 0;

    /// <summary>
    /// Sets the device manufacturer context information for telemetry events.
    /// </summary>
    /// <param name="deviceMake">A string that contains the manufacturer of the device, retrieved programmatically.</param>
    virtual void  SetDeviceMake(std::string const& deviceMake) = 0;

    /// <summary>
    /// Sets the device model context information (the name of the model of the device) for telemetry events.
    /// </summary>
    /// <param name="deviceModel">A string that contains the model of the device, retrieved programmatically.</param>
    virtual void  SetDeviceModel(std::string const& deviceModel) = 0;

    /// <summary>
    /// Sets the network cost context for telemetry events.
    /// </summary>
    /// <param name="networkCost">The cost of using data traffic on the network&mdash;as one of the ::NetworkCost enumeration values.</param>
    virtual void  SetNetworkCost(NetworkCost networkCost) = 0;

    /// <summary>
    /// Sets the network provider context for telemetry events.
    /// </summary>
    /// <param name="networkProvider">A string that contains the network provider used to connect to the current network, 
    /// retrieved programmatically.</param>
    virtual void  SetNetworkProvider(std::string const& networkProvider) = 0;

    /// <summary>
    /// Sets the network type (wired, wifi, etc.) context for telemetry events.
    /// </summary>
    /// <param name="networkType">The type of network, as one of the ::NetworkType enumeration values.</param>
    virtual void  SetNetworkType(NetworkType networkType) = 0;

    /// <summary>
    /// Sets the operating system name context for telemetry events.
    /// </summary>
    /// <param name="osName">A string that contains the operating system name, retrieved programmatically.</param>
    virtual void  SetOsName(std::string const& osName) = 0;

    /// <summary>
    /// Sets the operating system version context for telemetry events.
    /// </summary>
    /// <param name="osVersion">A string that contains the operating system version, retrieved programmatically.</param>
    virtual void  SetOsVersion(std::string const& osVersion) = 0;

    /// <summary>
    /// Sets the operating system build number context for telemetry events.
    /// </summary>
    /// <param name="osBuild">A string that contains the operating system build number, retrieved programmatically.</param>
    virtual void  SetOsBuild(std::string const& osBuild) = 0;

    /// <summary>
    /// Sets the user ID context for telemetry events.
    /// </summary>
    /// <param name="userId">A string that uniquely identifies a user in the application-specific user namespace.</param>
    /// <param name='piiKind'>The kind of personal identifiable information, as one of the ::PiiKind enumeration values. 
    /// If you don't supply this value, the default value of <i>PiiKind_Identity</i> is used. 
    /// Set this value to <i>PiiKind_None</i> to denote the user ID as being non-PII.</param>
    virtual void  SetUserId(std::string const& userId, PiiKind piiKind = PiiKind_Identity) = 0;

    /// <summary>
    /// Sets the user MSAID (Microsoft Account ID) context for telemetry events.
    /// </summary>
    /// <param name="userMsaId">A string that contains the MSA ID that identifies a user in the application-specific user namespace.</param>
    virtual void  SetUserMsaId(std::string const& userMsaId) = 0;

    /// <summary>
    /// Sets the user ANID (Anonymous ID) context information for telemetry events.
    /// </summary>
    /// <param name="userANID">A string that contains the ANID that identifies the user in the application-specific user namespace.</param>
    virtual void  SetUserANID(std::string const& userANID) = 0;

    /// <summary>
    /// Sets the advertising ID context for telemetry events.
    /// </summary>
    /// <param name="userAdvertingId">A string that contains the advertising ID of the user.</param>
    virtual void  SetUserAdvertisingId(std::string const& userAdvertingId) = 0;

    /// <summary>
    /// Sets the user's language context for telemetry events.
    /// </summary>
    /// <param name="locale">A string that contains the user's language in IETF language tag format, as described in RFC 4646.</param>
    virtual void  SetUserLanguage(std::string const& locale) = 0;

    /// <summary>
    /// Sets the user's time zone context for telemetry events.
    /// </summary>
    /// <param name="timeZone">A string that contains the user's time zone relative to UTC, in ISO 8601 time zone format.</param>
    virtual void  SetUserTimeZone(std::string const& timeZone) = 0;
};


}}} // namespace Microsoft::Applications::Telemetry

#endif //ISEMANTICCONTEXT_H