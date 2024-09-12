// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;

    public class QuestionExplanationError : Entity
    {
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(254, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Feedback { get; set; }

        public EnumFeedbackExplanation FeedbackExplanation { get; set; }
        public EnumProcessedStatus Status { get; set; }
        public Question? Question { get; set; }
        public Guid QuestionId { get; set; }
        public VideoResult? VideoResult { get; set; }
        public Guid? VideoResultId { get; set; }
        public Guid StudentId { get; set; }
    }
}
