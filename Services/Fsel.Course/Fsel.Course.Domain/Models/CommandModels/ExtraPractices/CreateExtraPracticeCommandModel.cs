// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.ExtraPractices
{
    using System.ComponentModel.DataAnnotations;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Models.CommandModels.Exercises;
    using Fsel.Course.Domain.Models.CommandModels.ExtraPractiveChapters;
    using Fsel.Course.Domain.Models.CommandModels.Videos;
    using Fsel.Shared.Enums;

    public class CreateExtraPracticeCommandModel
    {
        public string? Name { get; set; }

        [Required]
        public string? Code { get; set; }

        public string? InstructionContent { get; set; }
        public IList<string>? FilePaths { get; set; }
        public string? VideoLink { get; set; }
        public string? Author { get; set; }
        public string? Abstract { get; set; }

        [Required]
        public EnumExtraPracticeType Type { get; set; }

        [Required]
        public EnumCourseLevel CourseLevel { get; set; }

        public IList<CreateExtraPracticeChapterCommandModel>? ExtraPracticeChapters { get; set; }
        public IList<CreateExerciseCommandModel>? Exercises { get; set; } //Type : Video Embed,Exercise
        public CreateVideoCommandModel? Video { get; set; } //Type : Interactive Video
        // Type : Articles
    }
}
