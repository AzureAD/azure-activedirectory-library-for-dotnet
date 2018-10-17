// Copyright (c) Microsoft. All rights reserved.

#pragma once
#include "string"
#include "Version.hpp"

namespace ARIASDK_NS_BEGIN {

    class IStorage;

    /// <summary>
    /// The LogSessionData class represents the session cache.
    /// </summary>
    class LogSessionData
    {
    public:
        /// <summary>
        /// The LogSessionData constructor, taking a cache file path.
        /// </summary>
        LogSessionData(std::string const& cacheFilePath);

        /// <summary>
        /// Gets the time that this session began.
        /// </summary>
        /// <returns>A 64-bit integer that contains the time.</returns>
        unsigned long long getSesionFirstTime() const;

        /// <summary>
        /// Gets the SDK unique identifier.
        /// </summary>
        std::string& getSessionSDKUid();

    private:

        std::string                         m_sessionSDKUid;
        unsigned long long                  m_sessionFirstTimeLaunch;
        IStorage*                           m_sessionStorage;

        bool StartSessionStorage(std::string const& sessionpath);
        void StopSessionStorage();
        void PopulateSession();
        unsigned long long to_long(const char *string, size_t size);
    };


} ARIASDK_NS_END
