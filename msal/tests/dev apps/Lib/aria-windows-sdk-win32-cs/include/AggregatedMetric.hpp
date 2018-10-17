#pragma once
#include "ILogger.hpp"

// *INDENT-OFF*
namespace Microsoft { namespace Applications { namespace Telemetry {
namespace Models {
// *INDENT-ON*

/// <summary>
/// The AggregatedMetric class represents an aggregated metric event.
/// </summary>
class ARIASDK_LIBABI AggregatedMetric
{
  public:
    /// <summary>
    /// An AggregatedMetric constructor. Creates an aggregated metric instance for logging auto-aggregated metrics.
    /// </summary>
    /// <param name="name">A string that contains the name of the auto-aggregated metric.</param>
    /// <param name="units">A string that contains the units of the auto-aggregated metric.</param>
    /// <param name="intervalInSec">The polling cadence (in seconds) used to aggregate the metric.</param>
    /// <param name="eventProperties">The properties of the auto-aggregated metric event, as an EventProperties object.</param>
    /// <param name="pLogger">An ILogger interface pointer used to log this aggregated metric.</param>
    AggregatedMetric(std::string const& name,
        std::string const& units,
        unsigned const intervalInSec,
        EventProperties const& eventProperties,
        ILogger* pLogger = NULL);

    /// <summary>
    /// AggregatedMetric AggregatedMetric constructor that also takes an instance name, an object class, and an object ID. 
    /// Creates an aggregated metric instance for logging auto-aggregated metrics.
    /// </summary>
    /// <param name="name">A string that contains the name of the auto-aggregated metric.</param>
    /// <param name="units">A string that contains the units of the auto-aggregated metric.</param>
    /// <param name="intervalInSec">The polling cadence (in seconds) used to aggregate the metric.</param>
    /// <param name="instanceName">A string that contains the name of this metric instance - like for performance counter.</param>
    /// <param name="objectClass">A string that contains the object class for which this metric is trackings.</param>
    /// <param name="objectId">A string that contains the object ID for which this metric is trackings.</param>
    /// <param name="eventProperties">The properties of the auto-aggregated metric event, as an EventProperties object.</param>
    /// <param name="pLogger">An ILogger interface pointer used to log this aggregated metric.</param>
    AggregatedMetric(std::string const& name,
        std::string const& units,
        unsigned const intervalInSec,
        std::string const& instanceName,
        std::string const& objectClass,
        std::string const& objectId,
        EventProperties const& eventProperties,
        ILogger* pLogger = NULL);

    /// <summary>
    /// The AggregatedMetric destructor.
    /// </summary>
    ~AggregatedMetric();

    /// <summary>
    /// Pushes a single metric value for auto-aggregation.
    /// </summary>
    /// <param name="value">The metric value to push.</param>
    void PushMetric(double value);

  private:
    /// <summary>
    /// Actual implementation of AggregatedMetric.
    /// </summary>
    void* m_pAggregatedMetricImpl;
};

} // Models
}}}
