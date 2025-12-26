// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Entities.V1i1;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IEntities;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;

    public class ClassForumResult : Entity, ITokenResult
    {
        [MaxLength(5000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Content { get; set; }

        private string? _wordContent;

        [MaxLength(5000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? WordContent
        {
            get { return _wordContent; }
            set { _wordContent = value; WordCount = StringHelper.CountWords(value); }
        }

        private int? _wordCount;
        private EnumMediaType? _mediaType;

        public int? WordCount
        {
            get { return _wordCount == null ? StringHelper.CountWords(WordContent) : _wordCount; }
            set { _wordCount = value; }
        }

        [NotMapped]
        public int? TimeCount
        { get { return ClassForumResultFiles.Select(p => p.TimeCount).Sum(); } }

        [MaxLength(10000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? GradingAlFeedback { get; set; }

        public Guid? GradingTeacherId { get; set; }

        /// <summary>
        /// Trạng thái
        /// </summary>
        public EnumClassForumResultStatus? Status { get; set; }

        public EnumResultStatus ResultStatus { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid LessonResultId { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid StudentId { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid ClassForumId { get; set; }

        public ClassForum? ClassForum { get; set; }

        public LessonResult? LessonResult { get; set; }

        public Guid? CheckCsoId { get; set; }

        public DateTime? CheckStartDate { get; set; }

        public DateTime? GradingStartDate { get; set; }
        public double PercentModule { get; set; }
        public bool IsViewed { get; set; }
        public int? TokenFirstTime { get; set; }
        public int? TokenLastTime { get; set; }

        [NotMapped]
        public EnumMediaType? MediaType
        {
            get { return _mediaType.HasValue ? _mediaType : MediaHelper.GetMediaType(ClassForumResultFiles.Select(x => x.FilePath).FirstOrDefault()); }
            set { _mediaType = value; }
        }

        public EnumSubmissionCount? SubmissionCount { get; set; }

        /// <summary>
        /// Số câu trả lời đúng của Student
        /// </summary>
        [Range(0, 1000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public int CorrectCount { get; set; }

        /// <summary>
        /// Tổng số câu trả lời đúng
        /// </summary>
        [Range(0, 1000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public int CorrectTotal { get; set; }

        /// <summary>
        /// Phần trăm câu trả lời đúng
        /// </summary>
        private double _percent;

        [Range(0, 100, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public virtual double Percent
        {
            get
            {
                return CorrectTotal > 0 ? NumberHelper.GetPercent(CorrectCount, CorrectTotal) : _percent;
            }
            set { _percent = CorrectTotal > 0 ? NumberHelper.GetPercent(CorrectCount, CorrectTotal) : value; }
        }

        public string? SkillScoresStr { get; set; }

        [NotMapped]
        public IList<SkillScores>? SkillScores
        {
            get
            {
                return Common.Helpers.ConvertHelper.Deserialize<IList<SkillScores>>(SkillScoresStr);
            }
            set { SkillScoresStr = Common.Helpers.ConvertHelper.Serialize(value); }
        }

        public bool IsPendingSpeechToText { get; set; }

        public Guid? LessonModuleId { get; set; }
        public LessonModule? LessonModule { get; set; }
        public ICollection<ClassForumScore> ClassForumScores { get; set; } = new List<ClassForumScore>();
        public ICollection<ClassForumResultFile> ClassForumResultFiles { get; set; } = new List<ClassForumResultFile>();
        public ICollection<ClassForumResultRandom> ClassForumResultRandoms { get; set; } = new List<ClassForumResultRandom>();
        public ICollection<ClassForumDetailResult> ClassForumDetailResults { get; set; } = new List<ClassForumDetailResult>();
    }
}
