// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.Lessons.V1i1
{
    using Fsel.Shared.Enums;

    public class UpdateLessonCommandModel : CreateLessonCommandModel
    {
        public Guid Id { get; set; }

        public EnumStatus Status { get; set; }
    }
}
