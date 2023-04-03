// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.LessonNotes
{
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using System.ComponentModel.DataAnnotations;

    public class CreateLessonNoteCommandModel
    {
        public string? Name { get; set; }
        public string? Note { get; set; }
    }
}
