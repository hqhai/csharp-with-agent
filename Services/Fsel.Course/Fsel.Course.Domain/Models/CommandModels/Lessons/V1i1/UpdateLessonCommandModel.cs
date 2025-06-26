// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.Lessons.V1i1
{
    public class UpdateLessonCommandModel : CreateLessonCommandModel
    {
        public Guid Id { get; set; }
    }
}
