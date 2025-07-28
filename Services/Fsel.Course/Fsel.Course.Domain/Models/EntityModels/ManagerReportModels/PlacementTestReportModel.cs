// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.ManagerReportModels
{
    using Fsel.Common.Helpers;
    using Fsel.Shared.Enums;

    public class PlacementTestReportModel
    {
        public Guid StudentId { get; set; }
        public string? UserName { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? Birthday { get; set; }
        public string? SchoolName { get; set; }
        public string? SchoolGrade { get; set; }
        public string? SchoolClass { get; set; }
        public EnumCourseLevel? ChooseLevel { get; set; }

        public string? ChooseLevelStr
        {
            get
            {
                return ChooseLevel.HasValue ? ChooseLevel.Value.GetDescription() : null;
            }
        }

        public EnumCourseLevel? CurrentLevel { get; set; }

        public string? CurrentLevelStr
        {
            get
            {
                return CurrentLevel.HasValue ? CurrentLevel.Value.GetDescription() : null;
            }
        }

        public EnumCompletionStatus Status { get; set; }

        public string? StatusDescription
        {
            get
            {
                return Status.GetDescription();
            }
        }

        public DateTime? ExpiredPTDate { get; set; }
    }
}
