// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;

    public class PlacementTestResultReportGroupModel
    {
        public Guid StudentId { get; set; }
        public bool IsDonePT { get; set; }
        public EnumCourseLevel? CourseLevel { get; set; }
    }
}
