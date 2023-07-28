// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.ExtraPractiveChapters
{
    using System.ComponentModel.DataAnnotations;
    using Fsel.Course.Domain.Models.CommandModels.Exercises;

    public class CreateExtraPracticeChapterCommandModel
    {
        [Required]
        public string? Name { get; set; }

        public string? Description { get; set; }
        public int PageNumber { get; set; }
        public IList<CreateExerciseCommandModel>? Exercises { get; set; }
    }
}
