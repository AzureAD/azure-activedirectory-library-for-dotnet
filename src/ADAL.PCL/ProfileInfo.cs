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
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace Microsoft.Experimental.IdentityModel.Clients.ActiveDirectory
{
    internal class ProfileInfoClaim
    {
        public const string Version = "version";
        public const string Subject = "sub";
        public const string TenantId = "tid";
        public const string PreferredUsername = "preferred_username";
        public const string Name = "name";
    }

    [DataContract]
    internal class ProfileInfo
    {

        [DataMember(Name = ProfileInfoClaim.Version, IsRequired = false)]
        public string Version { get; set; }

        [DataMember(Name = ProfileInfoClaim.Subject, IsRequired = false)]
        public string Subject { get; set; }

        [DataMember(Name = ProfileInfoClaim.TenantId, IsRequired = false)]
        public string TenantId { get; set; }

        [DataMember(Name = ProfileInfoClaim.Name, IsRequired = false)]
        public string Name { get; set; }

        [DataMember(Name = ProfileInfoClaim.PreferredUsername, IsRequired = false)]
        public string PreferredUsername { get; set; }


        public static ProfileInfo Parse(string profileInfo)
        {
            ProfileInfo profileInfoBody = null;
            if (!string.IsNullOrWhiteSpace(profileInfo))
            {
                    try
                    {
                        byte[] profileInfoBytes = Base64UrlEncoder.DecodeBytes(profileInfo);
                        using (var stream = new MemoryStream(profileInfoBytes))
                        {
                            var serializer = new DataContractJsonSerializer(typeof(ProfileInfo));
                            profileInfoBody = (ProfileInfo)serializer.ReadObject(stream);
                        }
                    }
                    catch (SerializationException)
                    {
                        // We silently ignore the id token if exception occurs.   
                    }
                    catch (ArgumentException)
                    {
                        // Again, we silently ignore the id token if exception occurs.   
                    }
            }

            return profileInfoBody;
        }
    }
}
