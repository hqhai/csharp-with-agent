// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.CommandModels.BannerScopes
{
    using Fsel.Shared.Enums;
    using global::System.ComponentModel.DataAnnotations;

    public class UpdateBannerScopeCommandModel
    {
        public Guid? Id { get; set; }

        public bool IsPriority { get; set; }

        [Required]
        public IList<EnumTargetUser> TargetUsers { get; set; } = new List<EnumTargetUser>();

        public EnumCourseLevel CourseLevel { get; set; }

        [Required]
        public IList<EnumApplicableUserGroup> ApplicableUserGroups { get; set; } = new List<EnumApplicableUserGroup>();

        public IList<Guid>? CompetitionEventIds { get; set; }
    }
}
