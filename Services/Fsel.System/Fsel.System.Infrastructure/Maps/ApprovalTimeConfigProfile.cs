// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Maps
{
    using AutoMapper;
    using Fsel.Core.Extensions;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.Models.CommandModels.ApprovalTimeConfigs;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System.Collections.Generic;

    public class ApprovalTimeConfigProfile : Profile
    {
        public ApprovalTimeConfigProfile()
        {
            CreateMap<ApprovalTimeConfig, ApprovalTimeConfigModel>().IgnoreAllNonExisting();
            CreateMap<IList<ApprovalTimeConfig>, ApprovalTimeConfigCommandModel>().IgnoreAllNonExisting();
        }
    }
}
