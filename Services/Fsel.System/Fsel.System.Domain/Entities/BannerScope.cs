// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities
{
    using Fsel.Common.Helpers;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;
    using global::System.ComponentModel.DataAnnotations.Schema;

    public class BannerScope : Entity
    {
        public bool IsPriority { get; set; }

        public string? TargetUserStr { get; set; }

        [NotMapped]
        public IList<EnumTargetUser>? TargetUsers

        {
            get { return ConvertHelper.Deserialize<IList<EnumTargetUser>>(TargetUserStr); }
            set { TargetUserStr = ConvertHelper.Serialize(value); }
        }

        public EnumCourseLevel CourseLevel { get; set; }

        public string? ApplicableUserGroupsStr { get; set; }

        [NotMapped]
        public IList<EnumApplicableUserGroup>? ApplicableUserGroups

        {
            get { return ConvertHelper.Deserialize<IList<EnumApplicableUserGroup>>(ApplicableUserGroupsStr); }
            set { ApplicableUserGroupsStr = ConvertHelper.Serialize(value); }
        }

        public string? CompetitionEventIdsStr { get; set; }

        [NotMapped]
        public IList<Guid>? CompetitionEventIds

        {
            get { return ConvertHelper.Deserialize<IList<Guid>>(CompetitionEventIdsStr); }
            set { CompetitionEventIdsStr = ConvertHelper.Serialize(value); }
        }

        public Guid BannerId { get; set; }

        public Banner? Banner { get; set; }
    }
}
