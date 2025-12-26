// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IEntities;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;

    public class ClassForumDetailResult : Entity, ISubmissionCount
    {
        private const int MaxScore = 2;

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
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
        public EnumMediaType? MediaType
        {
            get { return _mediaType.HasValue ? _mediaType : MediaHelper.GetMediaType(ClassForumResultFiles.Select(x => x.FilePath).FirstOrDefault()); }
            set { _mediaType = value; }
        }

        public EnumSubmissionCount? SubmissionCount { get; set; }

        [NotMapped]
        public int? TimeCount
        { get { return ClassForumResultFiles.Select(p => p.TimeCount).Sum(); } }

        [NotMapped]
        public double Score { get; set; }

        [NotMapped]
        public double CorrectCount
        {
            get
            {
                double score = default;

                if (!string.IsNullOrEmpty(GradingAlFeedback))
                {
                    var classForumAIs = Common.Helpers.ConvertHelper.Deserialize<List<ClassForumAIModel>>(GradingAlFeedback);
                    if (classForumAIs != null && classForumAIs.Any())
                    {
                        score = classForumAIs.Sum(x => x.Score);
                    }
                }
                return score;
            }
        }

        [NotMapped]
        public double PronunciationScore
        {
            get
            {
                double score = default;

                if (!string.IsNullOrEmpty(PronunciationAlFeedback))
                {
                    var pronunciationAI = Common.Helpers.ConvertHelper.Deserialize<PronunciationAssessmentModel>(PronunciationAlFeedback);
                    if (pronunciationAI != null)
                    {
                        score = ScoreHelper.CalculatePronunciationScore(pronunciationAI.AccuracyScore, pronunciationAI.FluencyScore, pronunciationAI.ProsodyScore);
                    }
                }

                return score;
            }
        }

        [NotMapped]
        public double CorrectTotal
        {
            get
            {
                return CorrectCount + Score + PronunciationScore;
            }
        }

        [MaxLength(10000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? GradingAlFeedback { get; set; }

        public string? PronunciationAlFeedback { get; set; }

        public DateTime? ProcessDate { get; set; }
        public DateTime? CompletionDate { get; set; }

        public EnumClassForumResultStatus Status { get; set; }
        public Guid ClassForumResultId { get; set; }
        public ClassForumResult? ClassForumResult { get; set; }

        public ICollection<ClassForumResultFile> ClassForumResultFiles { get; set; } = new List<ClassForumResultFile>();

        public ICollection<ClassForumDetailResultHistory> ClassForumDetailResultHistories { get; set; } = new List<ClassForumDetailResultHistory>();

        public int RetryTime { get; set; }

        public bool IsForbiddenWork { get; set; }

        [MaxLength(10000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? GradingAiForbidden { get; set; }

        public bool IsForbiddenImage { get; set; }
    }
}
