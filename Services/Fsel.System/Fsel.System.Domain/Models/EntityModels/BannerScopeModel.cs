// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class BannerScopeModel : BaseModel
    {
        public bool IsPriority { get; set; }

        public IList<EnumTargetUser>? TargetUsers { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }

        public IList<EnumApplicableUserGroup>? ApplicableUserGroups { get; set; }

        public IList<Guid>? CompetitionEventIds { get; set; }
    }
}
