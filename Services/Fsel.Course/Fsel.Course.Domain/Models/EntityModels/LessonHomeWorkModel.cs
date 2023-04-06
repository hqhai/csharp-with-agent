// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Course.Domain.Entities;

    public class LessonHomeWorkModel
    {
        public Lesson? Lesson { get; set; }
        public HomeWork? HomeWork { get; set; }
        public Guid LessonId { get; set; }
        public Guid HomeWorkId { get; set; }
    }
}
