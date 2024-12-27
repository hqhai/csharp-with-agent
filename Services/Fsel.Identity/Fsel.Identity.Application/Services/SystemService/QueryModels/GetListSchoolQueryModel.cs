// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.SystemService.QueryModels
{
    using System;
    using System.Collections.Generic;
    using Fsel.Shared.Enums;

    public class GetListSchoolQueryModel
    {
        public EnumEducationLevel? EducationLevel { get; set; }
        public IList<Guid>? SchoolIds { get; set; }
    }
}
