//----------------------------------------------------------------------
// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
// Apache License 2.0
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//----------------------------------------------------------------------

using System;
using System.Runtime.Serialization;

namespace Microsoft.Experimental.IdentityModel.Clients.ActiveDirectory
{
    /// <summary>
    /// Contains information of a single user. This information is used for token cache lookup. Also if created with userId, userId is sent to the service when login_hint is accepted.
    /// </summary>
    [DataContract]
    public sealed class UserInfo
    {
        internal UserInfo()
        {            
        }

        internal UserInfo(UserInfo other)
        {
            this.UniqueId = other.UniqueId;
            this.DisplayableId = other.DisplayableId;
            this.Name = other.Name;
            this.Version = other.Version;
        }

        /// <summary>
        /// Gets identifier of the user authenticated during token acquisition. 
        /// </summary>
        [DataMember]
        public string UniqueId { get; internal set; }

        /// <summary>
        /// Gets a displayable value in UserPrincipalName (UPN) format. The value can be null.
        /// </summary>
        [DataMember]
        public string DisplayableId { get; internal set; }

        /// <summary>
        /// Gets given name of the user if provided by the service. If not, the value is null. 
        /// </summary>
        [DataMember]
        public string Name { get; internal set; }

        /// <summary>
        /// Gets family name of the user if provided by the service. If not, the value is null. 
        /// </summary>
        [DataMember]
        public string Version { get; internal set; }

    }
}
