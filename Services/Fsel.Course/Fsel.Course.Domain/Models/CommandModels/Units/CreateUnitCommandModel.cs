// Copyright (c) Atlantic. All rights reserved.

using Fsel.Shared.Enums;

namespace Fsel.Course.Domain.Models.CommandModels.Units
{
    public class CreateUnitCommandModel
    {
        public string? Name { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
        public IList<Guid>? LessonIds { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public Guid MockTestId { get; set; }
    }
}
