// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using Fsel.Core.Entities;
    using Fsel.Course.Domain.Entities.FlowConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IEntities;
    using Fsel.Shared.Enums;

    public class PlacementTestGroupResult : Entity, IModuleLifeCycle
    {
        public DateTime? ProcessDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public double Percent { get; set; }

        /// <summary>
        /// Level bài PT Bắt đầu
        /// </summary>
        public EnumPlacementTestLevel? ProcessLevel { get; set; }

        /// <summary>
        /// Level bài PT Cuối
        /// </summary>
        public EnumPlacementTestLevel? CompletionLevel { get; set; }

        /// <summary>
        /// Level hiện tại của học sinh
        /// </summary>
        public EnumCourseLevel? CurrentLevel { get; set; }

        /// <summary>
        /// Level hệ thống đề xuất
        /// </summary>
        public EnumCourseLevel? SuggetLevel { get; set; }

        /// <summary>
        /// Level học sinh chọn
        /// </summary>
        public EnumCourseLevel? ChooseLevel { get; set; }

        public DateTime? NewDate { get; set; }
        public EnumResultStatus Status { get; set; }
        public Guid StudentId { get; set; }

        public Flow? Flow { get; set; }
        public Guid? FlowId { get; set; }
        public ICollection<PlacementTestResult> PlacementTestResults { get; set; } = new List<PlacementTestResult>();
    }
}
