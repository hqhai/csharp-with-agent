// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.CommandModels.TeachingCosts
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class SaveTeachingCostCommandModel
    {
        public Guid? Id { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }

        public double WritingCost { get; set; }

        public double SpeapkingCost { get; set; }

        public double LiveLessonCost { get; set; }
    }
}
