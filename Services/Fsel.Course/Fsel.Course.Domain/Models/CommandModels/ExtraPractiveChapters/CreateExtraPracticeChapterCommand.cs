// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.ExtraPractiveChapters
{
    using System.ComponentModel.DataAnnotations;
    using Fsel.Course.Domain.Models.CommandModels.Exercises;

    public class CreateExtraPracticeChapterCommand
    {
        [Required]
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int PageNumber { get; set; }
        public IList<CreateExerciseCommandModel>? Exercises { get; set; }
    }
}
