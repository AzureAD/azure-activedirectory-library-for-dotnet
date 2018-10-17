// Copyright (c) Microsoft. All rights reserved.
#ifndef ARIA_ILOGGER_HPP
#define ARIA_ILOGGER_HPP

//#include "Version.hpp"
#include "ctmacros.hpp"
#include "Enums.hpp"
#include "EventProperties.hpp"
#include "ISemanticContext.hpp"
#include <stdint.h>
#include <string>
#include <vector>
#include <map>

// *INDENT-OFF*
namespace Microsoft { namespace Applications { namespace Telemetry {
// *INDENT-ON*

//Data Type Flags
#define MICROSOFT_KEYWORD_CRITICAL_DATA 0x0000800000000000 // Bit 47
#define MICROSOFT_KEYWORD_MEASURES      0x0000400000000000 // Bit 46
#define MICROSOFT_KEYWORD_TELEMETRY     0x0000200000000000 // Bit 45
//Core data Flags
#define MICROSOFT_EVENTTAG_CORE_DATA            0x00080000
//Latency Flags
#define MICROSOFT_EVENTTAG_COSTDEFERRED_LATENCY 0x00040000
#define MICROSOFT_EVENTTAG_REALTIME_LATENCY     0x00200000
#define MICROSOFT_EVENTTAG_NORMAL_LATENCY       0x00400000

/// <summary>
/// The PageActionData structure represents the data of a page action event.
/// </summary>
struct PageActionData
{
    /// <summary>
    /// [Required] The ID of the page view associated with this action.
    /// </summary>
    std::string pageViewId;

    /// <summary>
    /// [Required] A generic abstraction of the type of page action.
    /// </summary>
    ActionType actionType;

    /// <summary>
    /// [Optional] The type of physical action, as one of the RawActionType enumeration values.
    /// </summary>
    RawActionType rawActionType;

    /// <summary>
    /// [Optional] The type of input device that generates this page action.
    /// </summary>
    InputDeviceType inputDeviceType;

    /// <summary>
    /// [Optional] The ID of the item on which this action acts.
    /// </summary>
    std::string targetItemId;

    /// <summary>
    /// [Optional] The name of the data source item upon which this action acts.
    /// </summary>
    std::string targetItemDataSourceName;

    /// <summary>
    /// [Optional] The name of the data source category that the item belongs to.
    /// </summary>
    std::string targetItemDataSourceCategory;

    /// <summary>
    /// [Optional] The name of the data source collection that the item belongs to.
    /// </summary>
    std::string targetItemDataSourceCollection;

    /// <summary>
    /// [Optional] The name of the layout container the item belongs to.
    /// </summary>
    std::string targetItemLayoutContainer;

    /// <summary>
    /// [Optional] The relative ordering/ranking/positioning within the layout container.
    /// </summary>
    unsigned short targetItemLayoutRank;

    /// <summary>
    /// [Optional] The destination URI resulted by this action.
    /// </summary>
    std::string destinationUri;

    /// <summary>
    /// A constructor that takes a page view ID, and an action type.
    /// </summary>
    PageActionData(std::string const& pvId, ActionType actType)
      : pageViewId(pvId),
        actionType(actType),
        rawActionType(RawActionType_Unspecified),
        inputDeviceType(InputDeviceType_Unspecified),
        targetItemLayoutRank(0)
    {}
};

/// <summary>
/// The AggregatedMetricData structure contains the data of a precomputed aggregated metrics event.
/// </summary>
struct AggregatedMetricData
{
    /// <summary>
    /// [Required] The name of the precomputed aggregated metric.
    /// </summary>
    std::string name;

    /// <summary>
    /// [Required] The duration (length of time in microseconds) that this aggregated metric spans.
    /// </summary>
    long duration;

    /// <summary>
    /// [Required] The total count of metric observations aggregated in the duration.
    /// </summary>
    long count;

    /// <summary>
    /// [Optional] A string representing the units of measure of the aggregated metric.
    /// </summary>
    std::string units;

    /// <summary>
    /// [Optional] An instance name for the aggregated metric.
    /// </summary>
    std::string instanceName;

    /// <summary>m
    /// [Optional] A string that contains the object class upon which the aggregated metric is measured.
    /// </summary>
    std::string objectClass;

    /// <summary>
    /// [Optional] A string that contains the object ID upon which the Aggregated Metric is measured.
    /// </summary>
    std::string objectId;

    /// <summary>
    /// [Optional] The reported aggregated metrics.
    /// The types of aggregates are specified by the ::AggregateType enumeration.
    /// </summary>
    std::map<AggregateType, double> aggregates;

    /// <summary>
    /// [Optional] A standard map that contains a frequency table, which is an alternative way to summarize the observations (like a time series).
    /// </summary>
    std::map<long, long> buckets;

    /// <summary>
    /// An AggregatedMetricData constructor 
    /// that takes a string that contains the name of the aggregated metric,
    /// a long that contains the duration of the aggregation,
    /// and a long that contains the count of the number of occurrences.
    /// </summary>
    /// <param name='aggrName'>Name of the aggregated metric</param>
    /// <param name='aggrDuration'>Duration of the aggregation</param>
    /// <param name='aggrCount'>Number of occurrences</param>
    AggregatedMetricData(std::string const& aggrName, long aggrDuration, long aggrCount)
      : name(aggrName),
        duration(aggrDuration),
        count(aggrCount)
    {}
};

/// <summary>
/// ILogger interface for logging either semantic or custom event
/// </summary>
class ARIASDK_LIBABI ILogger
{

  public:
    /// <summary>
    /// The ILogger destructor.
    /// </summary>
    virtual ~ILogger() {}
 
    /// <summary>
    /// Gets an ISemanticContext interface through which you can specify the semantic context for this logger instance.
    /// </summary>
    /// <returns>An instance of the ISemanticContext interface.</returns>
    virtual ISemanticContext* GetSemanticContext() const = 0 ;

    /// <summary>
    /// Adds (or overrides) a property of the context associated with this logger instance,
    /// taking a string that contains the name of the context,
    /// a constant pointer to a character array that contains the property's string value,
    /// and tags it with the kind of customer content.
    /// </summary>
    /// <param name="name">A string that contains the name of the property.</param>
    /// <param name="value">A constant pointer to a character array that contains the property's string value.</param>
    /// <param name='ccKind'>One of the ::CustomerContentKind enumeration values.</param>
    virtual void SetContext(const std::string& name, const char value[], CustomerContentKind ccKind) = 0;

    /// <summary>
    /// Adds (or overrides) a property of the context associated with this logger instance,
    /// taking a string that contains the name of the context,
    /// a string that contains the property value,
    /// and tags it with the kind of customer content.
    /// </summary>
    /// <param name="name">A string that contains the name of the property.</param>
    /// <param name="value">A string that contains the property value.</param>
    /// <param name='ccKind'>One of the ::CustomerContentKind enumeration values.</param>
    virtual void SetContext(const std::string& name, const std::string &value, CustomerContentKind ccKind) = 0;

	/// <summary>
    /// Adds (or overrides) a property of the context associated with this logger instance,
    /// taking a string that contains the name of the context,
    /// a constant pointer to a character array that contains the property's string value,
    /// and tags the property with its PiiKind (Personal Identifiable Information kind).
	/// </summary>
    /// <param name="name">A string that contains the name of the property.</param>
    /// <param name="value">A constant pointer to a character array that contains the property's string value.</param>
	/// <param name='piiKind'>One of the ::PiiKind enumeration values. If you don't specify a value, 
	/// then the default <i>PiiKind_None</i> is used.</param>
	virtual void SetContext(const std::string& name, const char value[], PiiKind piiKind = PiiKind_None) = 0;

	/// <summary>
    /// Adds (or overrides) a property of the context associated with this logger instance,
    /// taking a string that contains the name of the context,
    /// a string that contains the property value,
    /// and tags the property with its PiiKind (Personal Identifiable Information kind).
	/// </summary>
    /// <param name="name">A string that contains the name of the property.</param>
    /// <param name="value">A string that contains the property value.</param>
	/// <param name='piiKind'>One of the ::PiiKind enumeration values. If you don't specify a value, 
	/// then the default <i>PiiKind_None</i> is used.</param>
	virtual void SetContext(const std::string& name, const std::string& value, PiiKind piiKind = PiiKind_None) = 0;

	/// <summary>
    /// Adds (or overrides) a property of the context associated with this logger instance,
    /// taking a string that contains the name of the context,
    /// a double that contains the property value,
    /// and tags the property with its PiiKind (Personal Identifiable Information kind).
	/// </summary>
    /// <param name="name">A string that contains the name of the property.</param>
	/// <param name="value">A double that contains the property value.</param>
	/// <param name='piiKind'>One of the ::PiiKind enumeration values. If you don't specify a value, 
	/// then the default <i>PiiKind_None</i> is used.</param>
	virtual void SetContext(const std::string& name, double value, PiiKind piiKind = PiiKind_None) = 0;

	/// <summary>
    /// Adds (or overrides) a property of the context associated with this logger instance,
    /// taking a string that contains the name of the context,
    /// an int64_t that contains the property value,
    /// and tags the property with its PiiKind (Personal Identifiable Information kind).
	/// </summary>
    /// <param name="name">A string that contains the name of the property.</param>
	/// <param name="value">An int64_t that contains the property value.</param>
	/// <param name='piiKind'>One of the ::PiiKind enumeration values. If you don't specify a value, 
	/// then the default <i>PiiKind_None</i> is used.</param>
	virtual void SetContext(const std::string& name, int64_t value, PiiKind piiKind = PiiKind_None) = 0;

	/// <summary>
    /// Adds (or overrides) a property of the context associated with this logger instance,
    /// taking a string that contains the name of the context,
    /// an int8_t that contains the property value,
    /// and tags the property with its PiiKind (Personal Identifiable Information kind).
	/// </summary>
    /// <param name="name">A string that contains the name of the property.</param>
	/// <param name="value">An int8_t that contains the property value.</param>
	/// <param name='piiKind'>One of the ::PiiKind enumeration values. If you don't specify a value, 
	/// then the default <i>PiiKind_None</i> is used.</param>
	virtual void SetContext(const std::string& name, int8_t  value, PiiKind piiKind = PiiKind_None) = 0;

	/// <summary>
    /// Adds (or overrides) a property of the context associated with this logger instance,
    /// taking a string that contains the name of the context,
    /// an int16_t that contains the property value,
    /// and tags the property with its PiiKind (Personal Identifiable Information kind).
	/// </summary>
    /// <param name="name">A string that contains the name of the property.</param>
	/// <param name="value">An int16_t that contains the property value.</param>
	/// <param name='piiKind'>One of the ::PiiKind enumeration values. If you don't specify a value, 
	/// then the default <i>PiiKind_None</i> is used.</param>
	virtual void SetContext(const std::string& name, int16_t value, PiiKind piiKind = PiiKind_None) = 0;

	/// <summary>
    /// Adds (or overrides) a property of the context associated with this logger instance,
    /// taking a string that contains the name of the context,
    /// an int32_t that contains the property value,
    /// and tags the property with its PiiKind (Personal Identifiable Information kind).
	/// </summary>
    /// <param name="name">A string that contains the name of the property.</param>
	/// <param name="value">An int32_t that contains the property value.</param>
	/// <param name='piiKind'>One of the ::PiiKind enumeration values. If you don't specify a value, 
	/// then the default <i>PiiKind_None</i> is used.</param>
	virtual void SetContext(const std::string& name, int32_t value, PiiKind piiKind = PiiKind_None) = 0;

	/// <summary>
    /// Adds (or overrides) a property of the context associated with this logger instance,
    /// taking a string that contains the name of the context,
    /// a uint8_t that contains the property value,
    /// and tags the property with its PiiKind (Personal Identifiable Information kind).
	/// </summary>
    /// <param name="name">A string that contains the name of the property.</param>
	/// <param name="value">A uint8_t that contains the property value.</param>
	/// <param name='piiKind'>One of the ::PiiKind enumeration values. If you don't specify a value, 
	/// then the default <i>PiiKind_None</i> is used.</param>
	virtual void SetContext(const std::string& name, uint8_t  value, PiiKind piiKind = PiiKind_None) = 0;

	/// <summary>
    /// Adds (or overrides) a property of the context associated with this logger instance,
    /// taking a string that contains the name of the context,
    /// a uint16_t that contains the property value,
    /// and tags the property with its PiiKind (Personal Identifiable Information kind).
	/// </summary>
    /// <param name="name">A string that contains the name of the property.</param>
	/// <param name="value">A uint16_t that contains the property value.</param>
	/// <param name='piiKind'>One of the ::PiiKind enumeration values. If you don't specify a value, 
	/// then the default <i>PiiKind_None</i> is used.</param>
	virtual void SetContext(const std::string& name, uint16_t value, PiiKind piiKind = PiiKind_None) = 0;

	/// <summary>
    /// Adds (or overrides) a property of the context associated with this logger instance,
    /// taking a string that contains the name of the context,
    /// a uint32_t that contains the property value,
    /// and tags the property with its PiiKind (Personal Identifiable Information kind).
	/// </summary>
    /// <param name="name">A string that contains the name of the property.</param>
	/// <param name="value">A uint32_t that contains the property value.</param>
	/// <param name='piiKind'>One of the ::PiiKind enumeration values. If you don't specify a value, 
	/// then the default <i>PiiKind_None</i> is used.</param>
	virtual void SetContext(const std::string& name, uint32_t value, PiiKind piiKind = PiiKind_None) = 0;

	/// <summary>
    /// Adds (or overrides) a property of the context associated with this logger instance,
    /// taking a string that contains the name of the context,
    /// a uint64_t that contains the property value,
    /// and tags the property with its PiiKind (Personal Identifiable Information kind).
	/// </summary>
    /// <param name="name">A string that contains the name of the property.</param>
	/// <param name="value">A uint64_t that contains the property value.</param>
	/// <param name='piiKind'>One of the ::PiiKind enumeration values. If you don't specify a value, 
	/// then the default <i>PiiKind_None</i> is used.</param>
	virtual void SetContext(const std::string& name, uint64_t value, PiiKind piiKind = PiiKind_None) = 0;

	/// <summary>
    /// Adds (or overrides) a property of the context associated with this logger instance,
    /// taking a string that contains the name of the context,
    /// a boolean that contains the property value,
    /// and tags the property with its PiiKind (Personal Identifiable Information kind).
	/// </summary>
    /// <param name="name">A string that contains the name of the property.</param>
	/// <param name="value">A boolean that contains the property value.</param>
	/// <param name='piiKind'>One of the ::PiiKind enumeration values. If you don't specify a value, 
	/// then the default <i>PiiKind_None</i> is used.</param>
	virtual void SetContext(const std::string& name, bool value, PiiKind piiKind = PiiKind_None) = 0;

	/// <summary>
    /// Adds (or overrides) a property of the context associated with this logger instance,
    /// taking a string that contains the name of the context,
    /// a .NET time_ticks_t that contains the property value,
    /// and tags the property with its PiiKind (Personal Identifiable Information kind).
	/// </summary>
    /// <param name="name">A string that contains the name of the property.</param>
	/// <param name="value">A .NET time_ticks_t that contains the property value.</param>
	/// <param name='piiKind'>One of the ::PiiKind enumeration values. If you don't specify a value, 
	/// then the default <i>PiiKind_None</i> is used.</param>
	virtual void SetContext(const std::string& name, time_ticks_t value, PiiKind piiKind = PiiKind_None) = 0;

	/// <summary>
    /// Adds (or overrides) a property of the context associated with this logger instance,
    /// taking a string that contains the name of the context,
    /// a GUID_t that contains the property value,
    /// and tags the property with its PiiKind (Personal Identifiable Information kind).
	/// </summary>
    /// <param name="name">A string that contains the name of the property.</param>
	/// <param name="value">A GUID_t that contains the property value.</param>
	/// <param name='piiKind'>One of the ::PiiKind enumeration values. If you don't specify a value, 
	/// then the default <i>PiiKind_None</i> is used.</param>
	virtual void SetContext(const std::string& name, GUID_t value, PiiKind piiKind = PiiKind_None) = 0;

    /// <summary>
    /// Logs the state of the application lifecycle.
    /// </summary>
    /// <param name="state">The state in the application's lifecycle, specified by one of the 
    /// ::AppLifecycleState enumeration values.</param>
    /// <param name="properties">Properties of this AppLifecycle event, specified using an EventProperties object.</param>
    virtual void LogAppLifecycle(AppLifecycleState state, EventProperties const& properties) = 0;

	/// <summary>
	/// Logs the state of the application session.
	/// </summary>
	/// <param name="state">The state in the application's lifecycle, as one of the SessionState enumeration values.</param>
    /// <param name="properties">Properties of this session event, specified using an EventProperties object.</param>
	virtual void LogSession(SessionState state, const EventProperties& properties) = 0;

    /// <summary>
    /// Logs the custom event with the specified name.
    /// </summary>
    /// <param name="name">A string that contains the name of the custom event.</param>
    virtual void LogEvent(std::string const& name) = 0;

    /// <summary>
    /// Logs a custom event with the specified name
    /// and properties.
    /// </summary>
    /// <param name="properties">Properties of this custom event, specified using an EventProperties object.</param>
    virtual void LogEvent(EventProperties const& properties) = 0;

    /// <summary>
    /// Logs a failure event - such as an application exception.
    /// </summary>
    /// <param name="signature">A string that contains the signature that identifies the bucket of the failure.</param>
    /// <param name="detail">A string that contains a description of the failure.</param>
    /// <param name="properties">Properties of this failure event, specified using an EventProperties object.</param>
    virtual void LogFailure(std::string const& signature,
        std::string const& detail,
        EventProperties const& properties) = 0;

    /// <summary>
    /// Logs a failure event - such as an application exception.
    /// </summary>
    /// <param name="signature">A string that contains the signature that identifies the bucket of the failure.</param>
    /// <param name="detail">A string that contains a description of the failure.</param>
    /// <param name="category">A string that contains the category of the failure - such as an application error, 
    /// application not responding, or application crash</param>
    /// <param name="id">A string that contains the identifier that uniquely identifies this failure.</param>
    /// <param name="properties">Properties of this failure event, specified using an EventProperties object.</param>
    virtual void LogFailure(std::string const& signature,
        std::string const& detail,
        std::string const& category,
        std::string const& id,
        EventProperties const& properties) = 0;

    /// <summary>
    /// Logs a page view event,
    /// taking a string that contains the event identifier,
    /// a string that contains a friendly name for the page,
    /// and properties of the event.<br>
    /// <b>Note:</b> A page view event is normally the result of a user action on a UI page
    /// such as a search query, a content request, or a page navigation.
    /// </summary>
    /// <param name="id">A string that contains an identifier that uniquely identifies this page.</param>
    /// <param name="pageName">A string that contains the friendly name of the page.</param>
    /// <param name="properties">Properties of this page view event, specified using an EventProperties object.</param>
    virtual void LogPageView(std::string const& id,
        std::string const& pageName,
        EventProperties const& properties) = 0;

    /// <summary>
    /// Logs a page view event, 
    /// taking a string that contains the event identifier, 
    /// a string that contains a friendly name for the page, 
    /// a string that contains the page category, 
    /// a string that contains the page's URI, 
    /// a string that contains the referring page's URI, 
    /// and properties of the event.<br>
    /// <b>Note:</b> A page view event is normally the result of a user action on a UI page
    /// such as a search query, a content request, or a page navigation.
    /// </summary>
    /// <param name="id">A string that contains the identifier that uniquely identifies this page.</param>
    /// <param name="pageName">A string that contains the friendly name of the page.</param>
    /// <param name="category">A string that contains the category to which this page belongs.</param>
    /// <param name="uri">A string that contains the URI of this page.</param>
    /// <param name="referrerUri">A string that contains the URI of the page that refers to this page.</param>
    /// <param name="properties">Properties of this page view event, specified using an EventProperties object.</param>
    virtual void LogPageView(std::string const& id,
        std::string const& pageName,
        std::string const& category,
        std::string const& uri,
        std::string const& referrerUri,
        EventProperties const& properties) = 0;

    /// <summary>
    /// Logs a page action event,
    /// taking a string that contains the page view identifier,
    /// the action type,
    /// and the action event properties.
    /// </summary>
    /// <param name="pageViewId">A string that contains an identifier that uniquely identifies the page view.</param>
    /// <param name="actionType">The generic type of the page action, specified as one of the ::ActionType enumeration values.</param>
    /// <param name="properties">Properties of this page action event, specified using an EventProperties object.</param>
    virtual void LogPageAction(std::string const& pageViewId,
        ActionType actionType,
        EventProperties const& properties) = 0;

    /// <summary>
    /// Logs a detailed page action event,
    /// taking a reference to the page action data,
    /// and the action event properties.
    /// </summary>
    /// <param name="pageActionData">Detailed information about the page action, contained in a PageActionData object.</param>
    /// <param name="properties">Properties of this page action event, contained in an EventProperties object.</param>
    virtual void LogPageAction(PageActionData const& pageActionData,
        EventProperties const& properties) = 0;

    /// <summary>
    /// Logs a sampled metric event - such as a performance counter,
    /// taking a name for the sampled metric, 
    /// a double that contains the value of the sampled metric, 
    /// a string that contains the units of measure of the sampled metric, 
    /// and a reference to an EventProperties object to hold the values.
    /// </summary>
    /// <param name="name">A string that contains the name of the sampled metric.</param>
    /// <param name="value">A double that holds the value of the sampled metric.</param>
    /// <param name="units">A string that contains the units of the metric value.</param>
    /// <param name="properties">Properties of this sampled metric event, specified using an EventProperties object.</param>
    virtual void LogSampledMetric(std::string const& name,
        double value,
        std::string const& units,
        EventProperties const& properties) = 0;

    /// <summary>
    /// Logs a sampled metric event - such as a performance counter, 
    /// taking a name for the sampled metric, 
    /// a double that contains the value of the sampled metric, 
    /// a string that contains the units of measure of the sampled metric, 
    /// a string that contains the name of the metric instance, 
    /// a string that contains the name of the object class, 
    /// a string that contains the object identifier, 
    /// and a reference to an EventProperties object to hold the values.
    /// </summary>
    /// <param name="name">A string that contains the name of the sampled metric.</param>
    /// <param name="value">A double that contains the value of the sampled metric.</param>
    /// <param name="units">A string that contains the units of the metric value.</param>
    /// <param name="instanceName">A string that contains the name of this metric instance. E.g., <i>performance counter</i>.</param>
    /// <param name="objectClass">A string that contains the object class for which this metric tracks.</param>
    /// <param name="objectId">A string that contains the object identifier for which this metric tracks.</param>
    /// <param name="properties">Properties of this sampled metric event, specified using an EventProperties object.</param>
    virtual void LogSampledMetric(std::string const& name,
        double value,
        std::string const& units,
        std::string const& instanceName,
        std::string const& objectClass,
        std::string const& objectId,
        EventProperties const& properties) = 0;

    /// <summary>
    /// Logs a precomputed aggregated metric event. For example, <i>queue length</i>.
    /// </summary>
    /// <param name="name">A string that contains the name of the aggregated metric.</param>
    /// <param name="duration">A long that contains the duration (in microseconds) over which this metric is aggregated.</param>
    /// <param name="count">A long that contains the count of the aggregated metric observations.</param>
    /// <param name="properties">Properties of this aggregated metric event, specified using an EventProperties object.</param>
    virtual void LogAggregatedMetric(std::string const& name,
        long duration,
        long count,
        EventProperties const& properties) = 0;

    /// <summary>
    /// Logs a precomputed aggregated metrics event, 
    /// taking a reference to an AggregatedMetricData object, 
    /// and a reference to a EventProperties object.
    /// </summary>
    /// <param name="metricData">Detailed information about the aggregated metric, contained in an AggregatedMetricData object.</param>
    /// <param name="properties">Properties of this aggregated metric event, specified in an EventProperties object.</param>
    virtual void LogAggregatedMetric(AggregatedMetricData const& metricData,
        EventProperties const& properties) = 0;

    /// <summary>
    /// Logs a trace event for troubleshooting.
    /// </summary>
    /// <param name="level">Level of the trace, as one of the TraceLevel enumeration values.</param>
    /// <param name="message">A string that contains the a description of the trace.</param>
    /// <param name="properties">Properties of this trace event, specified using an EventProperties object.</param>
    virtual void LogTrace(TraceLevel level,
        std::string const& message,
        EventProperties const& properties) = 0;

    /// <summary>
    /// Logs a user's state.
    /// </summary>
    /// <param name="state">The user's reported state, specified using one of the ::UserState enumeration values.</param>
    /// <param name="timeToLiveInMillis">A long that contains the duration (in milliseconds) for which the state reported is valid.</param>
    /// <param name="properties">Properties of this user state event, specified using an EventProperties object.</param>
    virtual void LogUserState(UserState state,
        long timeToLiveInMillis,
        EventProperties const& properties) = 0;
};


}}} // namespace Microsoft::Applications::Telemetry

#endif //ARIA_ILOGGER_H
