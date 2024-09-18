// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;

    public class QuestBoardParamModel
    {
        public Guid? CourseId { get; set; }

        public Guid? UnitId { get; set; }

        public string? NameUnit { get; set; }

        public int? DisplayOrder { get; set; }

        public Guid? LessonId { get; set; }

        public Guid? LessonResulId { get; set; }

        public Guid? ClassForumId { get; set; }

        public Guid? HomeWorkId { get; set; }

        public Guid? FinalTestId { get; set; }

        public Guid? MockTestId { get; set; }

        public EnumFeatureModule? FeatureModule { get; set; }
    }
}
