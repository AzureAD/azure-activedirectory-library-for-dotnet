
#ifndef ARIA_EVENTPROPERTY_HPP
#define ARIA_EVENTPROPERTY_HPP

#include "ctmacros.hpp"
#include "Enums.hpp"
#include <stdint.h>
#include <string>
#include <vector>
#include <map>
#include <ctime>
#include <cstdlib>
#include <cstdint>

#ifdef _WIN32
/* Required for GUID type helper function on Windows */
#include <ObjBase.h>
#endif

namespace Microsoft {
    namespace Applications {
        namespace Telemetry {

            /// <summary>
            /// The number of ticks per second.
            /// </summary>
            const uint64_t ticksPerSecond = 10000000UL;
            
            /// <summary>
            /// The UNIX epoch: Thursday, January, 01, 1970, 12:00:00 AM.
            /// </summary>
            const uint64_t ticksUnixEpoch = 0x089f7ff5f7b58000;

            /// <summary>
            /// The time_ticks_t structure encapsulates time in .NET ticks.
            /// </summary>
            /// <remarks>
            /// A single tick represents one hundred nanoseconds, or one ten-millionth of a second.
            /// There are 10,000 ticks in a millisecond, or 10 million ticks in a second.
            /// The value of this property represents the number of 100 nanosecond intervals that have
            /// elapsed since 12:00 AM, January, 1, 0001 (0:00 : 00 UTC on January 1, 0001, in
            /// the Gregorian calendar), which represents DateTime.MinValue.
            /// <b>Note:</b> This does not include the number  of ticks that are attributable to leap seconds.
            /// </remarks>
            struct ARIASDK_LIBABI time_ticks_t {
                /// <summary>
                /// A raw 64-bit unsigned integer that represents the number of .NET ticks.
                /// </summary>
                uint64_t ticks;

                /// <summary>
                /// The default constructor for instantiating an empty time_ticks_t object.
                /// </summary>
                time_ticks_t();

                /// <summary>
                /// Converts the number of .NET ticks into an instance of the time_ticks_t structure.
                /// </summary>
                time_ticks_t(uint64_t raw);

                /// <summary>
                /// Constructs a time_ticks_t object from a pointer to a time_t object from the standard library.
                /// <b>Note:</b> time_t time must contain a timestamp in UTC time.
                /// </summary>
                time_ticks_t(const std::time_t* time);

                /// <summary>
                /// The time_ticks_t copy constructor.
                /// </summary>
                time_ticks_t(const time_ticks_t& t);
            };

            /// <summary>
            /// The GUID_t structure represents the ARIA portable cross-platform implementation of a GUID (Globally Unique ID).
            /// </summary>
            /// <remarks>
            /// GUIDs identify objects such as interfaces, manager entry-point vectors (EPVs), and class objects.
            /// A GUID is a 128-bit value consisting of one group of eight hexadecimal digits, followed
            /// by three groups of four hexadecimal digits, each followed by one group of 12 hexadecimal digits.
            /// 
            /// The definition of this structure is the cross-platform equivalent to the 
            /// [Windows RPC GUID definition](https://msdn.microsoft.com/en-us/library/windows/desktop/aa373931%28v=vs.85%29.aspx).
            /// 
            /// <b>Note:</b> You must provide your own converter to convert from a <b>Windows RPC GUID</b> to a GUID_t.
            /// </remarks>
            struct ARIASDK_LIBABI GUID_t {
                /// <summary>
                /// Specifies the first eight hexadecimal digits of the GUID.
                /// </summary>
                uint32_t Data1;

                /// <summary>
                /// Specifies the first group of four hexadecimal digits.
                ///</summary>
                uint16_t Data2;

                /// <summary>
                /// Specifies the second group of four hexadecimal digits.
                /// </summary>
                uint16_t Data3;

                /// <summary>
                /// An array of eight bytes.
                /// The first two bytes contain the third group of four hexadecimal digits.
                /// The remaining six bytes contain the final 12 hexadecimal digits.
                /// </summary>
                uint8_t  Data4[8];

                /// <summary>
                /// The default GUID_t constructor.
                /// Creates a null instance of the GUID_t object (initialized to all zeros).
                /// {00000000-0000-0000-0000-000000000000}.
                /// </summary>
                GUID_t();

                /// <summary>
                /// A constructor that creates a GUID_t object from a hyphenated string.
                /// </summary>
                /// <param name="guid_string">A hyphenated string that contains the GUID (curly braces optional).</param>
                GUID_t(const char* guid_string);

                /// <summary>
                /// A constructor that creates a GUID_t object from a byte array.
                /// </summary>
                /// <param name="guid_bytes">A byte array.</param>
                /// <param name="bigEndian">
                /// A boolean value that specifies the byte order.<br>
                /// A value of <i>true</i> specifies the more natural human-readable order.<br>
                /// A value of <i>false</i> (the default) specifies the same order as the .NET GUID constructor.
                /// </param>
                GUID_t(const uint8_t guid_bytes[16], bool bigEndian = false);

                /// <summary>
                /// A constructor that creates a GUID_t object from three integers and a byte array.
                /// </summary>
                /// <param name="d1">An integer that specifies the first eight hexadecimal digits of the GUID.</param>
                /// <param name="d2">An integer that specifies the first group of four hexadecimal digits.</param>
                /// <param name="d3">An integer that specifies the second group of four hexadecimal digits.</param>
                /// <param name="v">A reference to an array of eight bytes.
                /// The first two bytes contain the third group of four hexadecimal digits.
                /// The remaining six bytes contain the final 12 hexadecimal digits.
                /// </param>
                GUID_t(int d1, int d2, int d3, const std::initializer_list<uint8_t> &v);
                
                /// <summary>
                /// The GUID_t copy constructor.
                /// </summary>
                /// <param name="guid">A GUID_t object.</param>
                GUID_t(const GUID_t& guid);

#ifdef _WIN32
                /// <summary>
                /// A constructor that creates a GUID_t object from a Windows GUID object.
                /// </summary>
                /// <param name="guid">A Windows GUID object.</param>
                GUID_t(GUID guid);

                /// <summary>
                /// Converts a standard vector of bytes into a Windows GUID object.
                /// </summary>
                /// <param name="bytes">A standard vector of bytes.</param>
                /// <returns>A GUID.</returns>
                static GUID convertUintVectorToGUID(std::vector<uint8_t> const& bytes);
             
#endif
                /// <summary>
                /// Converts this GUID_t to an array of bytes.
                /// </summary>
                /// <param name="guid_bytes">A uint8_t array of 16 bytes.</param>
                void to_bytes(uint8_t(&guid_bytes)[16]) const;
                
                /// <summary>
                /// Convert this GUID_t object to a string.
                /// </summary>
                /// <returns>This GUID_t object in a string.</returns>
                std::string to_string() const;

                /// <summary>
                /// Calculates the size of this GUID_t object.
                /// The output from this method is compatible with std::unordered_map.
                /// </summary>
                /// <returns>The size of the GUID_t object in bytes.</returns>
                std::size_t HashForMap() const;

                /// <summary>
                /// Tests to determine whether two GUID_t objects are equivalent (needed for maps).
                /// </summary>
                /// <returns>A boolean value that indicates success or failure.</returns>
                bool operator==(GUID_t const& other) const;
            };

            /// @cond INTERNAL_DOCS
            /// Excluded from public docs
            /// <summary>
            /// Declare the GuidMapHasher functor as the hasher when using GUID_t as a key in an unordered_map.
            /// </summary>
            struct GuidMapHasher
            {
                inline std::size_t operator()(GUID_t const& key) const
                {
                    return key.HashForMap();
                }
            };
            /// @endcond

            /// <summary>
            /// The EventProperty structure represents a C++11 variant object that holds an event property type 
            /// and an event property value.
            /// </summary>
            struct ARIASDK_LIBABI EventProperty
            {
                // <remarks>
                // With the concept of EventProperty value object we allow users implementing their
                // own type conversion system, which may subclass and provides an implementation of
                // to_string method
                // </remarks>
            public:

                /// <summary>
                /// This anonymous enumeration contains a set of values that specify the types
                /// that are supported by the Aria collector.
                /// </summary>
                enum
                {
                    /// <summary>
                    /// A string.
                    /// </summary>
                    TYPE_STRING,
                    /// <summary>
                    /// A 64-bit signed integer.
                    /// </summary>
                    TYPE_INT64,
                    /// <summary>
                    /// A double.
                    /// </summary>
                    TYPE_DOUBLE,
                    /// <summary>
                    /// A date/time object represented in .NET ticks.
                    /// </summary>
                    TYPE_TIME,
                    /// <summary>
                    /// A boolean.
                    /// </summary>
                    TYPE_BOOLEAN,
                    /// <summary>
                    /// A GUID.
                    /// </summary>
                    TYPE_GUID,
                } type;

                /// <summary>
                /// The kind of PII (Personal Identifiable Information) for an event.
                /// </summary>
                PiiKind piiKind;

                 /// <summary>
                 /// The kind of customer content associated with an event.
                 /// </summary>
                CustomerContentKind ccKind = CustomerContentKind_None;

                /// <summary>
                /// The value of the event property.
                ///
                /// union<br>
                /// {<br>
                ///&nbsp;&nbsp;&nbsp;&nbsp;char*&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;as_string;<br>
                ///&nbsp;&nbsp;&nbsp;&nbsp;int64_t&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;as_int64;<br>
                ///&nbsp;&nbsp;&nbsp;&nbsp;double&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;as_double;<br>
                ///&nbsp;&nbsp;&nbsp;&nbsp;bool&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;as_bool;<br>
                ///&nbsp;&nbsp;&nbsp;&nbsp;GUID_t&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;as_guid;<br>
                ///&nbsp;&nbsp;&nbsp;&nbsp;time_ticks_t&nbsp;as_time_ticks;<br>
                /// }
                /// </summary>
                union
                {
                    char*        as_string;
                    int64_t      as_int64;
                    double       as_double;
                    bool         as_bool;
#if defined(_MSC_VER) && (_MSC_VER < 1900)
                }; // end of anonymous union for vs2013
#endif
                /* *** C++11 compatibility quirk of vs2013 ***
                 * Unfortunately vs2013 is not fully C++11 compliant: it does not support complex
                 * variant objects within anonymous union. So the structure has to occcupy more
                 * RAM by placing these two members outside of the union.
                 * The rest of code logic remains the same. */
                    GUID_t       as_guid;
                    time_ticks_t as_time_ticks;
#if !(defined(_MSC_VER) && (_MSC_VER < 1900))
                }; // end of anonymous union for other C++11 compilers (sigh)
#endif

                /// <summary>
                /// A debug routine that returns a string representation of the type name.
                /// </summary>
                /// <param name="typeId">An unsigned integer that contains the type ID.</param>
                /// <returns>A pointer to the string representation of the type name.</returns>
                static const char *type_name(unsigned typeId);
            
                /// <summary>
                /// The EventProperty copy constructor.
                /// </summary>
                /// <param name="source">The EventProperty object to copy.</param>
                EventProperty(const EventProperty& source);

                /// <summary>
                /// The EventProperty move constructor.
                /// </summary>
                /// <param name="source">The EventProperty object to move.</param>
                EventProperty(EventProperty&& source);

                /// <summary>
                /// The EventProperty equalto operator.
                /// </summary>
                bool operator==(const EventProperty& source) const;

                /// <summary>
                /// An EventProperty assignment operator that takes an EventProperty object.
                /// </summary>
                EventProperty& operator=(const EventProperty& source);

                /// <summary>
                /// An EventProperty assignment operator that takes a string value.
                /// </summary>
                EventProperty& operator=(const std::string& value);

                /// <summary>
                /// An EventProperty assignment operator that takes a character pointer to a string.
                /// </summary>
                EventProperty& operator=(const char *value);

                /// <summary>
                /// An EventProperty assignment operator that takes an int64_t value.
                /// </summary>
                EventProperty& operator=(int64_t value);

                // All other integer types get converted to int64_t
#ifdef _WIN32
                /// <summary>
                /// An EventProperty assignment operator that takes a long value.
                /// </summary>
                EventProperty& operator=(long    value);
#endif
                /// <summary>
                /// An EventProperty assignment operator that takes an int8_t value.
                /// </summary>
                EventProperty& operator=(int8_t  value);

                /// <summary>
                /// An EventProperty assignment operator that takes an int16_t value.
                /// </summary>
                EventProperty& operator=(int16_t value); 

                /// <summary>
                /// An EventProperty assignment operator that takes an int32_t value.
                /// </summary>
                EventProperty& operator=(int32_t value); 

                /// <summary>
                /// An EventProperty assignment operator that takes a uint8_t value.
                /// </summary>
                EventProperty& operator=(uint8_t  value); 

                /// <summary>
                /// An EventProperty assignment operator that takes a uint16_t value.
                /// </summary>
                EventProperty& operator=(uint16_t value); 

                /// <summary>
                /// An EventProperty assignment operator that takes a uint32_t value.
                /// </summary>
                EventProperty& operator=(uint32_t value); 

                /// <summary>
                /// An EventProperty assignment operator that takes a uint64_t value.
                /// </summary>
                EventProperty& operator=(uint64_t value); 

                /// <summary>
                /// An EventProperty assignment operator that takes a double.
                /// </summary>
                EventProperty& operator=(double value);

                /// <summary>
                /// An EventProperty assignment operator that takes a boolean value.
                /// </summary>
                EventProperty& operator=(bool value);

                /// <summary>
                /// An EventProperty assignment operator that takes a time_ticks_t value.
                /// </summary>
                EventProperty& operator=(time_ticks_t value);

                /// <summary>
                /// An EventProperty assignment operator that takes a GUID_t value.
                /// </summary>
                EventProperty& operator=(GUID_t value);

                /// <summary>
                /// Clears the object values, deallocating memory when needed.
                /// </summary>
                void clear();

                /// <summary>
                /// The EventProperty destructor.
                /// </summary>
                virtual ~EventProperty();

                /// <summary>
                /// The EventProperty default constructor.
                /// </summary>
                EventProperty();

                /// <summary>
                /// The EventProperty constructor, taking a character pointer to a string, and the kind of personal identifiable information.
                /// </summary>
                /// <param name="value">A constant character pointer to a string.</param>
                /// <param name="piiKind">The kind of personal identifiable information.</param>
                EventProperty(const char* value, PiiKind piiKind = PiiKind_None);

                /// <summary>
                /// The EventProperty constructor, taking a string value, and the kind of personal identifiable information.
                /// </summary>
                /// <param name="value">A string value.</param>
                /// <param name="piiKind">The kind of personal identifiable information.</param>
                EventProperty(const std::string& value, PiiKind piiKind = PiiKind_None);

                /// <summary>
                /// The EventProperty constructor, taking a character pointer to a string, and the kind of customer content.
                /// </summary>
                /// <param name="value">A constant character pointer to a string.</param>
                /// <param name="ccKind">The kind of customer content.</param>
                EventProperty(const char* value, CustomerContentKind ccKind);

                /// <summary>
                /// The EventProperty constructor, taking a string value, and the kind of customer content.
                /// </summary>
                /// <param name="value">A string value.</param>
                /// <param name="ccKind">The kind of customer content.</param>
                EventProperty(const std::string& value, CustomerContentKind ccKind);

                /// <summary>
                /// EventProperty constructor, taking an int64 value, and the kind of personal identifiable information.
                /// </summary>
                /// <param name="value">An int64_t value.</param>
                /// <param name="piiKind">The kind of personal identifiable information.</param>
                EventProperty(int64_t value, PiiKind piiKind = PiiKind_None);

                /// <summary>
                /// EventProperty constructor, that takes a double value, and the kind of personal identifiable information.
                /// </summary>
                /// <param name="value">A double value.</param>
                /// <param name="piiKind">The kind of personal identifiable information.</param>
                EventProperty(double value, PiiKind piiKind = PiiKind_None);

                /// <summary>
                /// EventProperty constructor, taking time in .NET ticks, and the kind of personal identifiable information.
                /// </summary>
                /// <param name="value">The time_ticks_t value - time in .NET ticks</param>
                /// <param name="piiKind">The kind of personal identifiable information.</param>
                EventProperty(time_ticks_t value, PiiKind piiKind = PiiKind_None);

                /// <summary>
                /// EventProperty constructor, taking a boolean value, and the kind of personal identifiable information.
                /// </summary>
                /// <param name="value">A boolean value.</param>
                /// <param name="piiKind">The kind of personal identifiable information.</param>
                EventProperty(bool value, PiiKind piiKind = PiiKind_None);

                /// <summary>
                /// EventProperty constructor, taking a GUID_t, and the kind of personal identifiable information.
                /// </summary>
                /// <param name="value">A GUID_t value.</param>
                /// <param name="piiKind">The kind of personal identifiable information.</param>
                EventProperty(GUID_t value, PiiKind piiKind = PiiKind_None);

                // All other integer types are converted to int64_t.
#ifdef _WIN32
                /// <summary>
                /// EventProperty constructor, taking a long value, and the kind of personal identifiable information.
                /// </summary>
                /// <param name="value">A long value.</param>
                /// <param name="piiKind">The kind of personal identifiable information.</param>
                EventProperty(long value, PiiKind piiKind = PiiKind_None); 
#endif
                /// <summary>
                /// EventProperty constructor, taking an int8_t value, and the kind of personal identifiable information.
                /// </summary>
                /// <param name="value">An int8_t value.</param>
                /// <param name="piiKind">The kind of personal identifiable information.</param>
                EventProperty(int8_t value, PiiKind piiKind = PiiKind_None); 

                /// <summary>
                /// EventProperty constructor, taking an int16_t value, and the kind of personal identifiable information.
                /// </summary>
                /// <param name="value">An int16_t value.</param>
                /// <param name="piiKind">The kind of personal identifiable information.</param>
                EventProperty(int16_t value, PiiKind piiKind = PiiKind_None); 

                /// <summary>
                /// EventProperty constructor, taking an int32_t value, and the kind of personal identifiable information.
                /// </summary>
                /// <param name="value">An int32_t value.</param>
                /// <param name="piiKind">The kind of personal identifiable information.</param>
                EventProperty(int32_t value, PiiKind piiKind = PiiKind_None); 

                /// <summary>
                /// EventProperty constructor, taking a uint8_t value, and the kind of personal identifiable information.
                /// </summary>
                /// <param name="value">A uint8_t value.</param>
                /// <param name="piiKind">The kind of personal identifiable information.</param>
                EventProperty(uint8_t value, PiiKind piiKind = PiiKind_None); 

                /// <summary>
                /// EventProperty constructor, taking a uint16_t value, and the kind of personal identifiable information.
                /// </summary>
                /// <param name="value">A uint16_t value.</param>
                /// <param name="piiKind">The kind of personal identifiable information.</param>
                EventProperty(uint16_t value, PiiKind piiKind = PiiKind_None);

                /// <summary>
                /// EventProperty constructor, taking a uint32_t value, and the kind of personal identifiable information.
                /// </summary>
                /// <param name="value">A uint32_t value.</param>
                /// <param name="piiKind">The kind of personal identifiable information.</param>
                EventProperty(uint32_t value, PiiKind piiKind = PiiKind_None); 

                /// <summary>
                /// EventProperty constructor, taking a uint64_t value, and the kind of personal identifiable information.
                /// </summary>
                /// <param name="value">A uint64_t value.</param>
                /// <param name="piiKind">The kind of personal identifiable information.</param>
                EventProperty(uint64_t value, PiiKind piiKind = PiiKind_None); 

                /// <summary>
                /// Returns <i>true</i> when the type is string AND the value is empty.
                /// </summary>
                bool empty();

                /// <summary>
                /// Returns a string representation of this object.
                /// </summary>
                virtual std::string to_string() const;

            };

        }
    }
}

#endif //ARIA_EVENTPROPERTY_HPP
