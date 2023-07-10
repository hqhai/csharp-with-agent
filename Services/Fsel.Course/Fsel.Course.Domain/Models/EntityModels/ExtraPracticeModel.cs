// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Enums;
    using Fsel.Shared.Enums;

    public class ExtraPracticeModel : BaseModel
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? InstructionContent { get; set; }
        public string? BookFilePath { get; set; }
        public string? BookCoverPath { get; set; }
        public string? BookBackgroundPath { get; set; }
        public string? Author { get; set; }
        public string? Abstract { get; set; }
        public bool IsActive { get; set; }
        public string? VideoLink { get; set; }
        public EnumExtraPracticeType Type { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public Guid? VideoId { get; set; }
        public Guid? MockTestId { get; set; }
        public Guid? PlacementTestId { get; set; }
        public IList<ExtraPracticeChapterModel>? ExtraPracticeChapters { get; set; }
        public VideoModel? Video { get; set; }
        public IList<ExerciseModel>? Exercises { get; set; }
    }
}
