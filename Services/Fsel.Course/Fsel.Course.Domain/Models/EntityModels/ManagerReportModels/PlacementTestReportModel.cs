// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.ManagerReportModels
{
    using Fsel.Common.Helpers;
    using Fsel.Shared.Enums;

    public class PlacementTestReportModel
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? Birthday { get; set; }
        public string? SchoolName { get; set; }
        public string? SchoolGrade { get; set; }
        public string? SchoolClass { get; set; }
        public EnumCourseLevel? ChooseLevel { get; set; }
        public EnumCourseLevel? CurrentLevel { get; set; }
        public EnumCompletionStatus Status { get; set; }

        public string? StatusDescription
        {
            get
            {
                return Status.GetDescription();
            }
        }

        public DateTime? ExpirePTDate { get; set; }
    }
}
