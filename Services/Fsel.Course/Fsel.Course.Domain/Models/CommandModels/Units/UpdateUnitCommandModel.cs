// Copyright (c) Atlantic. All rights reserved.

using Fsel.Shared.Enums;
using Fsel.Core.Base.BaseModels;

namespace Fsel.Course.Domain.Models.CommandModels.Units
{
    public class UpdateUnitCommandModel : BaseCommandModel
    {
        public string? Name { get; set; }

        public string? Code { get; set; }

        public IList<Guid>? LessonIds { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }

        public Guid MockTestId { get; set; }
    }
}
