// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.SystemService.QueryModels
{
    using Fsel.Shared.Enums;

    public class GetSchoolsQueryModel
    {
        public EnumEducationLevel? EducationLevel { get; set; }
        public IList<Guid>? SchoolIds { get; set; }
    }
}
