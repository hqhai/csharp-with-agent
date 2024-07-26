// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    using System;
    using Fsel.Shared.Enums;

    public class FeatureAccessTimeInvokeModel
    {
        public Guid? UnitId { get; set; }

        public Guid? LessonId { get; set; }

        public Guid? ObjectId { get; set; }
        public Guid? CourseId { get; set; }

        public EnumFeature Type { get; set; }

    }
}
