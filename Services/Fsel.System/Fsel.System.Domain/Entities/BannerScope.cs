// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities
{
    using Fsel.Common.Helpers;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;
    using global::System.ComponentModel.DataAnnotations.Schema;

    public class BannerScope : Entity
    {
        public EnumApplicableUserGroup ApplicableUser { get; set; }

        public bool IsPriority { get; set; }

        public string? TargetUserStr { get; set; }

        [NotMapped]
        public IList<EnumTargetUser>? TargetUsers

        {
            get { return ConvertHelper.Deserialize<IList<EnumTargetUser>>(TargetUserStr); }
            set { TargetUserStr = ConvertHelper.Serialize(value); }
        }

        public EnumCourseLevel? CourseLevel { get; set; }

        public Guid? CompetitionEventId { get; set; }

        public Guid BannerId { get; set; }

        public Banner? Banner { get; set; }
    }
}
