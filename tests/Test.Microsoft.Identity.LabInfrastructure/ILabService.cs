﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;

namespace Microsoft.Identity.Test.LabInfrastructure
{
    public interface ILabService
    {
        Task<LabResponse> GetLabResponseFromApiAsync(UserQuery query);
        Task<LabResponse> CreateTempLabUserAsync();
    }
}
