// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Common.Enums.ErrorCodes;
    using Common.Helpers;
    using Fsel.Core.Entities;
    using TestConfigs;

    public class CourseChangingHistory : Entity
    {
        public Guid StudentId { get; set; }
        public Guid? ToLevelId { get; set; }
        public Guid? SelectedLevelId { get; set; }
        public Guid ToProgramId { get; set; }
        public Guid? SelectedProgramId { get; set; }
        public Guid? ToCourseResultId { get; set; }
        public CourseResult? ToCourseResult { get; set; }

        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? FromInfoStr { get; set; }

        [NotMapped]
        public FromInfo? FromInfo
        {
            get => FromInfoStr.Deserialize<FromInfo>();
            set => FromInfoStr = value.Serialize();
        }

        public Guid? PtResultId { get; set; }
        public TestGroupResult? PtTestResult { get; set; }
        public EnumChangeCourseAction Action { get; set; }
        public EnumChangingStatus Status { get; set; }

        [MaxLength(2000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Description { get; set; }
    }

    public class FromInfo
    {
        public Guid? LevelId { get; set; }
        public Guid? ProgramId { get; set; }
        public Guid? CourseResultId { get; set; }
    }

    public enum EnumChangeCourseAction
    {
        None,
        NotAllow,
        OverlapLevel,
        ChangeAndStartPt,
        ChangeDirectly,
        ChangeDirectlyBecauseByPass,
        SwitchToExistedCourse
    }

    public enum EnumChangingStatus
    {
        None,
        InprogressSelectProgram,
        InProgressPt,
        InProgressSelectCourse,
        Completed
    }
}
