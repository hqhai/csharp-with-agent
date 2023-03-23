// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System;

    public class LessonVideoModel
    {
        public Guid LessonId { get; set; }

        public Guid VideoId { get; set; }
    }
}
