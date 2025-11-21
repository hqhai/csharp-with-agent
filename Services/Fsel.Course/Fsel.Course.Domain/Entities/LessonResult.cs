// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities.V1i1;

    public class LessonResult : BaseScoreResult
    {
        /// <summary>
        /// Lưu ý tóm tắt
        /// </summary>
        [MaxLength(2000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? SummaryNote { get; set; }

        public Course? Course { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid CourseId { get; set; }

        public Unit? Unit { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid UnitId { get; set; }

        [Range(0, 100, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public override double Percent { get; set; }

        public Lesson? Lesson { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid LessonId { get; set; }

        [NotMapped]
        public VideoResult? VideoResult { get; set; }

        public UnitResult? UnitResult { get; set; }
        public Guid? UnitResultId { get; set; }

        public CourseResult? CourseResult { get; set; }
        public Guid? CourseResultId { get; set; }

        public UnitModule? UnitModule { get; set; }
        public Guid? UnitModuleId { get; set; }

        public ICollection<LessonNote> LessonNotes { get; set; } = new List<LessonNote>();
        public ICollection<VideoResult> VideoResults { get; set; } = new List<VideoResult>();
        public ICollection<HomeWorkResult> HomeWorkResults { get; set; } = new List<HomeWorkResult>();
        public ICollection<ClassForumResult> ClassForumResults { get; set; } = new List<ClassForumResult>();
        public ICollection<DocumentResult> DocumentResults { get; set; } = new List<DocumentResult>();
    }
}
