/*
 * DebugEventSource.hpp
 */
#pragma once

#ifndef DEBUGEVENTS_HPP
#define DEBUGEVENTS_HPP

#include <cstdint>
#include <map>
#include <vector>
#include <string>
#include <functional>
#include <algorithm>

#include <chrono>

#ifndef __cplusplus_cli
#include <atomic>
#endif

#include <cstdio>
#include <cstdlib>
#include "Version.hpp"

namespace ARIASDK_NS_BEGIN
{

    /// <summary>
    /// The DebugEventType enumeration contains a set of values that specify the types debug events supported by the Aria C++ SDK.
    /// </summary>
    typedef enum DebugEventType
    {
        /// <summary>API call: logEvent.</summary>
        EVT_LOG_EVENT = 0x01000000,
        /// <summary>API call: logAppLifecycle.</summary>
        EVT_LOG_LIFECYCLE = 0x01000001,
        /// <summary>API call: logFailure.</summary>
        EVT_LOG_FAILURE = 0x01000002,
        /// <summary>API call: logPageView.</summary>
        EVT_LOG_PAGEVIEW = 0x01000004,
        /// <summary>API call: logPageAction.</summary>
        EVT_LOG_PAGEACTION = 0x01000005,
        /// <summary>API call: logSampledMetric.</summary>
        EVT_LOG_SAMPLEMETR = 0x01000006,
        /// <summary>API call: logAggregatedMetric.</summary>
        EVT_LOG_AGGRMETR = 0x01000007,
        /// <summary>API call: logTrace.</summary>
        EVT_LOG_TRACE = 0x01000008,
        /// <summary>API call: logUserState.</summary>
        EVT_LOG_USERSTATE = 0x01000009,
        /// <summary>API call: logSession.</summary>
        EVT_LOG_SESSION = 0x0100000A,
        /// <summary>Event(s) added to queue.</summary>
        EVT_ADDED = 0x01001000,
        /// <summary>Event(s) cached in offline storage.</summary>
        EVT_CACHED = 0x02000000,
        /// <summary>Event(s) dropped.</summary>
        EVT_DROPPED = 0x03000000,
        /// <summary>Event(s) filtered.</summary>
        EVT_FILTERED = 0x03000001,

        /// <summary>Event(s) sent.</summary>
        EVT_SENT = 0x04000000,
        /// <summary>Event(s) send failed.</summary>
        EVT_SEND_FAILED = 0x04000001,
        /// <summary>Event(s) send retry.</summary>
        EVT_SEND_RETRY = 0x04000002,
        /// <summary>Event(s) retry drop.</summary>
        EVT_SEND_RETRY_DROPPED = 0x04000003,

        /// <summary>Event(s) rejected. E.g., Failed regexp check, or missing event name.</summary>
        EVT_REJECTED = 0x05000000,
        /// <summary>HTTP stack connection failure.</summary>
        EVT_CONN_FAILURE = 0x0A000000,
        /// <summary>HTTP stack failure.</summary>
        EVT_HTTP_FAILURE = 0x0A000001,
        /// <summary>Compression failed.</summary>
        EVT_COMPRESS_FAILED = 0x0A000002,
        /// <summary>HTTP stack unknown host.</summary>
        EVT_UNKNOWN_HOST = 0x0A000003,
        /// <summary>HTTP response error.</summary>
        EVT_HTTP_ERROR = 0x0B000000,
        /// <summary>HTTP response 200 OK.</summary>
        EVT_HTTP_OK = 0x0C000000,
        /// <summary>Network state change.</summary>
        EVT_NET_CHANGED = 0x0D000000,
        /// <summary>Storage full.</summary>
        EVT_STORAGE_FULL = 0x0E000000,
        /// <summary>Unknown error.</summary>
        EVT_UNKNOWN = 0xDEADBEEF,

        /// <summary>TODO: Allow us to monitor all events types rather than just specific event types.</summary>
        //EVT_MASK_ALL        = 0xFFFFFFFF // We don't allow the 'all' handler at this time.
    } DebugEventType;

    /// <summary>The DebugEvent class represents a debug event object.</summary>
    class DebugEvent
    {

    public:
        /// <summary>The debug event sequence number.</summary>
        uint64_t seq;
        /// <summary>The debug event timestamp.</summary>
        uint64_t ts;
        /// <summary>The debug event type.</summary>
        DebugEventType type;
        /// <summary>[optional] Parameter 1 (depends on debug event type).</summary>
        size_t param1;
        /// <summary>[optional] Parameter 2 (depends on debug event type).</summary>
        size_t param2;
        /// <summary>[optional] The debug event data (depends on debug event type).</summary>
        void* data;
        /// <summary>[optional] The size of the debug event data (depends on debug event type).</summary>
        size_t size;
        /// <summary>DebugEvent The default DebugEvent constructor.</summary>
        DebugEvent() : seq(0), ts(0), type(EVT_UNKNOWN), param1(0), param2(0), data(NULL), size(0) {}
    };

    /// <summary>
    /// The DebugEventListener class allows applications to register ARIA SDK debug callbacks
    /// for debugging and unit testing (not recommended for use in a production environment).
    /// 
    /// Customers can implement this abstract class to track when certain events
    /// happen under the hood in the ARIA SDK. The callback is synchronously executed
    /// within the context of the ARIA worker thread.
    /// </summary>
    class DebugEventListener
    {

    public:
        /// <summary>The DebugEventListener constructor.</summary>
        virtual void OnDebugEvent(DebugEvent &evt) = 0;
        
        /// <summary>The DebugEventListener destructor.</summary>
        virtual ~DebugEventListener() {};
    };

/// @cond INTERNAL_DOCS
    /// <summary>The DebugEventSource class represents a debug event source.</summary>
    class DebugEventSource
    {
    public:
        /// <summary>The DebugEventSource constructor.</summary>
        DebugEventSource() : seq(0) {}

        /// <summary>The DebugEventSource destructor.</summary>        
        virtual ~DebugEventSource() {};

        /// <summary>Adds an event listener for the specified debug event type.</summary>
        virtual void AddEventListener(DebugEventType type, DebugEventListener &listener);

        /// <summary>Removes previously added debug event listener for the specified type.</summary>
        virtual void RemoveEventListener(DebugEventType type, DebugEventListener &listener);

        /// <summary>Dispatches an event of the specified type to a client callback.</summary>
        virtual bool DispatchEvent(DebugEventType type);

        /// <summary>Dispatches the specified event to a client callback.</summary>
        virtual bool DispatchEvent(DebugEvent &evt);
    private:
        /// <summary>A collection of debug event listeners.</summary>
        std::map<unsigned, std::vector<DebugEventListener*> > listeners;
        uint64_t seq;
    };
/// @endcond

} ARIASDK_NS_END

#endif
