// Copyright (c) Microsoft. All rights reserved.

#pragma once
#include "Version.hpp"
#include "Enums.hpp"
#include <map>
#include <string>
#include <vector>

///@cond INTERNAL_DOCS

// *INDENT-OFF*
namespace Microsoft { namespace Applications { namespace Telemetry {
// *INDENT-ON*

/// <summary>
/// The HttpHeaders class contains a set of HTTP headers.
/// </summary>
class HttpHeaders : public std::multimap<std::string, std::string>
{
  public:
    /// <summary>
    /// A multimap constant bidirectional iterator.
    /// </summary>
    using std::multimap<std::string, std::string>::const_iterator;

    /// <summary>
    /// An multimap bidirectional iterator.
    /// </summary>
    using std::multimap<std::string, std::string>::iterator;

    /// <summary>
    /// A std::pair<const Key, T>.
    /// </summary>
    using std::multimap<std::string, std::string>::value_type;

  public:
    /// <summary>
    /// The HttpHeaders default constructor.
    /// </summary>
    HttpHeaders()
    {
    }

    /// <summary>
    /// Inserts a name/value pair, and removes elements with the same name.
    /// </summary>
    /// <param name="name">A string that contains the name.</param>
    /// <param name="value">A string that contains the value.</param>
    void set(std::string const& name, std::string const& value)
    {
        auto range = equal_range(name);
        auto hint = erase(range.first, range.second);
        insert(hint, std::make_pair(name, value));
    }

    /// <summary>
    /// Inserts a name/value pair into the multimap.
    /// </summary>
    void add(std::string const& name, std::string const& value)
    {
        insert(std::make_pair(name, value));
    }

    /// <summary>
    /// Gets a string value given a name.
    /// </summary>
    /// <param name="name">A string that contains the name.</param>
    /// <returns>A string that contains the value associated with the name.</returns>
    std::string const& get(std::string const& name) const
    {
        auto it = find(name);
        return (it != end()) ? it->second : m_empty;
    }

    /// <summary>
    /// Tests whether the multimap contains the specified name.
    /// </summary>
    /// <param name="name">A string that contains the name to look for.</param>
    /// <returns>A boolean that indicates success (true), or failure (false).</returns>
    bool has(std::string const& name) const
    {
        auto it = find(name);
        return (it != end());
    }

    using std::multimap<std::string, std::string>::begin;
    using std::multimap<std::string, std::string>::end;

  protected:
    std::string m_empty;
};

/// <summary>
/// The IHttpRequest class represents a Request object.
/// Individual HTTP client implementations can implement the request object in
/// the most efficient way. Either fill the request first, and then issue
/// the underlying request, or create the real request immediately, and then forward
/// the methods and set the individual parameters directly one by one.
/// </summary>
class IHttpRequest
{
  public:

    /// <summary>
    /// The IHttpRequest class destructor.
    /// </summary>
    virtual ~IHttpRequest() {}

    /// <summary>
    /// Gets the request ID.
    /// </summary>
    virtual const std::string& GetId() const = 0;

    /// <summary>
    /// The set method.
    /// </summary>
    /// <param name="method">A string that contains the the name of the method to set (e.g., <i>GET</i>).</param>
    virtual void SetMethod(std::string const& method) = 0;

    /// <summary>
    /// Sets the request URI.
    /// </summary>
    /// <param name="url">A string that contains the URI to set.</param>
    virtual void SetUrl(std::string const& url) = 0;

    /// <summary>
    /// Gets the request headers.
    /// </summary>
    /// <returns>The HTTP headers in an HttpHeaders object.</returns>
    virtual HttpHeaders& GetHeaders() = 0;

    /// <summary>
    /// Sets the request body.
    /// </summary>
    /// <param name="body">A standard vector that contains the message body.</param>
    virtual void SetBody(std::vector<uint8_t>& body) = 0;

    /// <summary>
    /// Sets the request priority.
    /// </summary>
    /// <param name="priority">The event priority, as one of the EventPriority enumeration values.</param>
    virtual void SetPriority(EventPriority priority) = 0;

    /// <summary>
    /// Gets the size of the request message body.
    /// </summary>
    /// <returns>The size of the request message body, in bytes.</returns>
    virtual size_t GetSizeEstimate() const = 0;
};

/// <summary>
/// The IHttpResponse class represents a Response object.
/// Individual HTTP client implementations can implement the response object
/// in the most efficient way. Either copy all of the underlying data to a new
/// structure, and provide it to the callback; or keep the real
/// response data around, forward the methods, and retrieve the individual
/// values directly one by one.
/// </summary>
class IHttpResponse
{
  public:

    /// <summary>
    /// The IHttpResponse class destructor.
    /// </summary>
    virtual ~IHttpResponse() {}

    /// <summary>
    /// Gets the response ID.
    /// </summary>
    /// <returns>A string that contains the response ID.</returns>
    virtual const std::string& GetId() const = 0;

    /// <summary>
    /// Get the response result code.
    /// </summary>
    /// <returns>The result, as one of the HttpResult enumeration values.</returns>
    virtual HttpResult GetResult() const = 0;

    /// <summary>
    /// Gets the response status code.
    /// </summary>
    /// <returns>An unsigned integer that contains the status code.</returns>
    virtual unsigned GetStatusCode() const = 0;

    /// <summary>
    /// Gets the response headers.
    /// </summary>
    /// <returns>The headers in a reference to an HttpHeaders object.</returns>
    virtual const HttpHeaders& GetHeaders() const = 0;

    /// <summary>
    /// Gets the response body.
    /// </summary>
    /// <returns>A vector that contains the message body.</returns>
    virtual const std::vector<uint8_t>& GetBody() const = 0;
};

/// Excluded from public docs
/// <summary>
/// The SimpleHttpRequest class represents a simple request.
/// </summary>
class SimpleHttpRequest : public IHttpRequest {
  public:
    /// <summary>
    /// A string that contains the request ID.
    /// </summary>
    std::string          m_id;

    /// <summary>
    /// A string that contains the type of request method.
    /// </summary>
    std::string          m_method;

    /// <summary>
    /// A string that contains the request URI.
    /// </summary>
    std::string          m_url;

    /// <summary>
    /// The request headers in a HttpHeaders object.
    /// </summary>
    HttpHeaders          m_headers;

    /// <summary>
    /// A vector that contains the request message body.
    /// </summary>
    std::vector<uint8_t> m_body;

    /// <summary>
    /// The event priority as one of the EventPriority enumeration values.
    /// </summary>
    EventPriority        m_priority;

  public:

    /// <summary>
    /// A SimpleHttpRequest class constructor that takes a request ID, and initializes the request method to 
    /// <i>GET</i>, and the event priority to <i>EventPriority_Unspecified</i>.
    /// </summary>
    /// <param name="id">A string that contains the request ID.</param>
    SimpleHttpRequest(std::string const& id)
      : m_id(id),
        m_method("GET"),
        m_priority(EventPriority_Unspecified)
    {
    }


    /// <summary>
    /// The SimpleHttpRequest destructor.
    /// </summary>
    virtual ~SimpleHttpRequest()
    {
    }

    /// <summary>
    /// Gets the HTTP request ID.
    /// </summary>
    /// <returns>A string that contains the request ID.</returns>
    virtual const std::string& GetId() const override
    {
        return m_id;
    }

    /// <summary>
    /// Sets the request method (e.g.., <i>GET</i>).
    /// </summary>
    /// <param name="method">A string that contains the method.</param>
    virtual void SetMethod(std::string const& method) override
    {
        m_method = method;
    }

    /// <summary>
    /// Sets the HTTP request URI.
    /// </summary>
    /// <param name="url">A string that contains the URI.</param>
    virtual void SetUrl(std::string const& url) override
    {
        m_url = url;
    }

    /// <summary>
    /// Gets the HTTP request headers.
    /// </summary>
    /// <returns>The headers, in a reference to a HttpHeaders object.</returns>
    virtual HttpHeaders& GetHeaders() override
    {
        return m_headers;
    }

    /// <summary>
    /// Sets the request body.
    /// </summary>
    /// <param name="body">The request body in a vector of uint8_ts.</param>
    virtual void SetBody(std::vector<uint8_t>& body) override
    {
        m_body = std::move(body);
    }

    /// <summary>
    /// Sets the event priority.
    /// </summary>
    /// <param name="priority">The event priority as one of the EventPriority enumeration values.</param>
    virtual void SetPriority(EventPriority priority) override
    {
        m_priority = priority;
    }

    /// <summary>
    /// Gets an estimate of the size of the request message, including the method size, the URI size, and the message body size.
    /// </summary>
    virtual size_t GetSizeEstimate() const override
    {
        // Not accounting for a few more chars here and there, assuming the
        // protocol & hostname part of the URL reasonably offsets that.
        size_t size = m_method.size() + m_url.size() + m_body.size();
        for (auto const& header : m_headers) {
            size += header.first.size() + header.second.size() + 4;
        }
        return size;
    }
};

/// Excluded from public docs
/// <summary>
/// The SimpleHttpResponse class represents a simple response.
/// </summary>
class SimpleHttpResponse : public IHttpResponse {
  public:

    /// <summary>
    /// The response ID.
    /// </summary>
    std::string          m_id;

    /// <summary>
    /// The response result, as an HttpResult enumeration value.
    /// </summary>
    HttpResult           m_result;

    /// <summary>
    /// The response status code.
    /// </summary>
    unsigned             m_statusCode;

    /// <summary>
    /// The response headers, and an HttpHeaders object.
    /// </summary>
    HttpHeaders          m_headers;

    /// <summary>
    /// A vector of uint8_ts that contains the response body.
    /// </summary>
    std::vector<uint8_t> m_body;

  public:
    /// <summary>
    /// A SimpleHttpResponse constructor that takes a string that contains the response ID, 
    /// and initializes the response result to <i>HttpResult_LocalFailure</i>, and 
    /// the status code to <i>0</i>.
    /// </summary>
    /// <param name="id">A string that contains the response message ID.</param>
    SimpleHttpResponse(std::string const& id)
      : m_id(id),
        m_result(HttpResult_LocalFailure),
        m_statusCode(0)
    {
    }

    /// <summary>
    /// The SimpleHttpResponse class destructor.
    /// </summary>
    virtual ~SimpleHttpResponse()
    {
    }

    /// <summary>
    /// Gets the HTTP response message Id.
    /// </summary>
    /// <returns>A string that contains the response message ID.</returns>
    virtual const std::string& GetId() const override
    {
        return m_id;
    }

    /// <summary>
    /// Gets the HTTP message result as one of the HttpResult enumeration values.
    /// </summary>
    virtual HttpResult GetResult() const override
    {
        return m_result;
    }

    /// <summary>
    /// Gets the response status code.
    /// </summary>
    /// <returns>The status code in an unsigned integer.</returns>
    virtual unsigned GetStatusCode() const override
    {
        return m_statusCode;
    }

    /// <summary>
    /// Gets the HTTP response message headers.
    /// </summary>
    /// <returns>The response message headers in a reference to an HttpHeaders object.</returns>
    virtual const HttpHeaders& GetHeaders() const override
    {
        return m_headers;
    }

    /// <summary>
    /// Gets the response message body.
    /// </summary>
    /// <returns>A vector of uint8_ts that contains the message body.</returns>
    virtual const std::vector<uint8_t>& GetBody() const override
    {
        return m_body;
    }
};

/// <summary>
/// The IHttpResponseCallback class receives HTTP client responses.
/// </summary>
class IHttpResponseCallback
{
  public:
    /// <summary>
    /// The IHttpResponseCallback class destructor.
    /// </summary>
    virtual ~IHttpResponseCallback() {}

    /// <summary>
    /// Called when an HTTP request completes.
    /// The passed response object contains details about the exact way the
    /// request finished (HTTP status code, headers, content, error codes
    /// etc.). The ownership of the response object is transferred to the
    /// callback object. It can store it for later if necessary. Finally, it
    /// must be deleted using its virtual destructor.
    /// </summary>
    /// <param name="response">The object that contains the response data.</param>
    virtual void OnHttpResponse(IHttpResponse const* response) = 0;
};

/// <summary>
/// The IHttpClient class is the interface for HTTP client implementations.
/// </summary>
class IHttpClient
{
  public:
    virtual ~IHttpClient() {}

    /// <summary>
    /// Creates an empty HTTP request object.
    /// The created request object has only its ID prepopulated. Other fields
    /// must be set by the caller. The request object can then be sent
    /// using SendRequestAsync(). If you are not going to use the request object, 
    /// then you can delete it safely using its virtual destructor.
    /// </summary>
    /// <returns>An HTTP request object for you to prepare.</returns>
    virtual IHttpRequest* CreateRequest() = 0;

    /// <summary>
    /// Begins an HTTP request.
    /// The method takes ownership of the passed request, and can destroy it before
    /// returning to the caller. Do not access the request object in any
    /// way after this invocation, and do not delete it.
    /// The callback object is always called, even if the request is 
    /// cancel led, or if an error occurs immediately during sending. In the
    /// latter case, the OnHttpResponse() callback is called before this
    /// method returns. You must keep the callback object alive until its
    /// OnHttpResponse() callback is called. It will never be used twice, so
    /// after you use it - you can safely delete it.
    /// </summary>
    /// <param name="request">The filled request object returned earlier by
    /// CreateRequest()</param>
    /// <param name="callback">The callback to receive the response.</param>
    virtual void SendRequestAsync(IHttpRequest* request, IHttpResponseCallback* callback) = 0;

    /// <summary>
    /// Cancels an HTTP request.
    /// The caller must provide a string ID returned earlier by request->GetId().
    /// The request is cancelled asynchronously. The caller must still 
    /// wait for the relevant OnHttpResponse() callback (it can just come
    /// earlier with some "aborted" error status).
    /// </summary>
    /// <param name="id">A string that contains the ID of the request to cancel.</param>
    virtual void CancelRequestAsync(std::string const& id) = 0;
};

/// @endcond

}}} // namespace Microsoft::Applications::Telemetry
