// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.SystemService.Models
{
    using Fsel.Shared.Enums;

    public class GetTokenConfigsQueryModel
    {
        public EnumTokenFeature Feature { get; set; }
        public IList<EnumTokenMission>? Missions { get; set; }
    }
}
