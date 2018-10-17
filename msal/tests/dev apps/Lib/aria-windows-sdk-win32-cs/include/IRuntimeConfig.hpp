// Copyright (c) Microsoft. All rights reserved.

#pragma once
#include "Version.hpp"
#include "ILogger.hpp"
#include <string>
#include <map>

// *INDENT-OFF*
namespace Microsoft { namespace Applications { namespace Telemetry {
// *INDENT-ON*

///@cond INTERNAL_DOCS

class IRuntimeConfig {
  public:
    virtual ~IRuntimeConfig() {}

    /// <summary>
    /// Sets the default configuration values.
    /// </summary>
    /// <remarks>
    /// Called once by LogManager::Initialize() to provide the runtime configuration 
    /// with the default values set in LogConfiguration, or hardcoded in the
    /// library. The passed object is kept alive and does change until
    /// the last call to any other method of the custom IRuntimeConfig
    /// implementation, which can use the default values for settings it does
    /// not need to change (e.g., collector URI) or override (e.g.,
    /// the maximum offline storage size).
    /// </remarks>
    /// <param name="defaultConfig">A reference to a default runtime configuration object.
    /// object</param>
    virtual void SetDefaultConfig(IRuntimeConfig& defaultConfig) = 0;

    /// <summary>
    /// Gets the URI of the collector (where telemetry events are sent).
    /// </summary>
    /// <remarks>
    /// <b>Note:</b> Since this method is called for every event, 
    /// you can change the URI dynamically.
    /// </remarks>
    /// <returns>A string that contains the collector URI.</returns>
    virtual std::string GetCollectorUrl() const = 0;

    /// <summary>
    /// Adds extension fields (created by the configuration provider) to an
    /// event.
    /// </summary>
    /// <remarks>
    /// Examples of extension fields are the current configuration ID (e.g., the ETag of
    /// the last received ECS configuration), or a comma-separated list of active
    /// experimentation bits (e.g., ECS experiment IDs).
    /// </remarks>
    /// <param name="extension">A map of extension fields to fill.</param>
    /// <param name="experimentationProject">A string that contains the name of the project.</param>
    /// <param name="eventName">A string that contains the name of the event.</param>
    virtual void DecorateEvent(std::map<std::string, std::string>& extension, std::string const& experimentationProject, std::string const& eventName) const = 0;

    /// <summary>
    /// Gets the priority for an event.
    /// </summary>
    /// <param name="tenantId">A string that contains the ID of the tenant. 
    /// <b>Note:</b> This is not the same as the tenant token.</param>
    /// <param name="eventName">A string that contains the event name (as it is used in
    /// EventProperties.</param>
    /// <returns>The event priority, as one of the ::EventPriority enumeration values.</returns>
    virtual EventPriority GetEventPriority(std::string const& tenantId  = std::string(), std::string const& eventName = std::string()) const = 0;

    /// <summary>
    /// Gets the tenant token for the Aria SDK itself.
    /// </summary>
    /// <returns>A string that contains the tenant token.</returns>
    virtual std::string GetMetaStatsTenantToken() const = 0;

    /// <summary>
    /// Gets the interval for sending meta-statistics (about the operation of
    /// the Aria SDK itself).
    /// </summary>
    /// <returns>An unsigned integer that contains the interval (measured in seconds).</returns>
    virtual unsigned GetMetaStatsSendIntervalSec() const = 0;

    /// <summary>
    /// Gets the maximum size of the offline storage file. You can use this to trigger file trimming.
    /// See also <see cref="GetOfflineStorageResizeThresholdPct"/>.
    /// </summary>
     /// <returns>An unsigned integer that contains the size, in bytes.</returns>
    virtual unsigned GetOfflineStorageMaximumSizeBytes() const = 0;

    /// <summary>
    /// Gets the percentage of events dropped when the maximum size of the database is exceeded.
    /// See also <see cref="GetOfflineStorageMaximumSizeBytes"/>.
    /// </summary>
    /// <remarks>
    /// The top <i>N</i> percent of events is sorted by priority, 
    /// and then by the timestamp of events that will be dropped.
    /// </remarks>
    /// <returns>An unsigned integer that contains the percentage of events that will be dropped.</returns>
    virtual unsigned GetOfflineStorageResizeThresholdPct() const = 0;

    /// <summary>
    /// Gets the maximum number of retries after which an event that failed to be uploaded, 
    /// is discarded.
    /// </summary>
    /// <remarks>
    /// This method is called each time an event fails to be uploaded 
    /// (except if the failure was the result of an Internet connectivity problem).
    /// </remarks>
    /// <returns>An unsigned integer that contains the maximum number of retries.</returns>
    virtual unsigned GetMaximumRetryCount() const = 0;

    /// <summary>
    /// Gets the backoff configuration for the number of event upload retries (in the case of upload errors).
    /// The term backoff refers to the length of time to wait after a failed send attempt&mdash;to reattempt another send.
    /// </summary>
    /// <remarks>
    /// Each time there is an upload failure the the backoff time is doubled (i.e., 1 s., 2 s., 4 s., etc.).
    /// <br><br>
    /// <b>Note:</b> When the configuration changes, the current backoff time is reset to the initial value.
    /// The supported policy is exponential backoff with jitter (introduced deviation).
    /// The configuration string is in the following format:
    ///
    ///     E,<initialDelayMs>,<maximumDelayMs>,<multiplier>,<jitter>
    ///
    /// where the delays are integers (in milliseconds), and the multiplier and jitter 
    /// values are floating-points.
    /// </remarks>
    /// <returns>A string that contains the backoff configuration.</returns>
    virtual std::string GetUploadRetryBackoffConfig() const = 0;

    /// <summary>
    /// Determines whether the compression of HTTP requests is enabled.
    /// </summary>
    /// <remarks>
    /// This method is called every time packaged events are sent, and therefore, 
    /// you can enable/disable compression dynamically.
    /// </remarks>
    /// <returns>A boolean value that indicates that either compression is enabled (<i>true</i>), or not (<i>false</i>).</returns>
    virtual bool IsHttpRequestCompressionEnabled() const = 0;

    /// <summary>
    /// Gets the minimum bandwidth necessary to start an upload.
    /// </summary>
    /// <remarks>
    /// The returned value is used only if the
    /// telemetry library is configured to use a 
    /// <see cref="IBandwidthController"/> implementation. This method is called
    /// each time an upload is prepared.
    /// </remarks>
    /// <returns>An unsigned integer that contains the minimum bandwidth, in bytes per second.</returns>
    virtual unsigned GetMinimumUploadBandwidthBps() const = 0;

    /// <summary>
    /// Gets the maximum payload size for an upload request.
    /// </summary>
    /// <remarks>
    /// The size limit is enforced on uncompressed request data, and does not take
    /// overhead (like HTTPS handshake or HTTP headers) into account. This method
    /// is called every time events are packaged for uploading.<br>
    /// <b>Note:</b> If the returned value stops the library from sending even just one
    /// event, then the limit is ignored in order to still send data.
    /// </remarks>
    /// <returns>An unsigned integer that contains the maximum payload size in bytes.</returns>
    virtual unsigned GetMaximumUploadSizeBytes() const = 0;

    /// <summary>
    /// Sets the priority for an event.
    /// </summary>
    /// <remarks>
    /// <b>Note:</b> Event priority set through this method is just a suggestion, 
    /// and might be ignored. It can be overridden by the
    /// configuration implementation (e.g., based on ECS) and the value set
    /// through this method might have no effect.
    /// </remarks>
    /// <param name="tenantId">A string that contains the tenant ID (not a tenant token).</param>
    /// <param name="eventName">A string that contains the event name.</param>
    /// <param name="priority">The event priority, as an ::EventPriority enumeration value.</param>
    virtual void SetEventPriority(std::string const& tenantId, std::string const& eventName, EventPriority priority) = 0;

	/// <summary>
	/// Determines if clock skew is enabled.
	/// </summary>
	/// <returns>A boolean value that indicates that clock skew is either enabled (true), or not (false).</returns>
	virtual bool IsClockSkewEnabled() const = 0;
};

/// @endcond

}}} // namespace Microsoft::Applications::Telemetry
