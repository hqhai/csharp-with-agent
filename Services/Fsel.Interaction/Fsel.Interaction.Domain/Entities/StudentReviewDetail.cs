// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;

    public class StudentReviewDetail : Entity
    {
        /// <summary>
        /// Loai Cau Hoi
        /// </summary>
        public EnumReviewQuestionType ReviewQuestionType { get; set; }

        /// <summary>
        /// VoteStars
        /// </summary>
        public double VoteStars { get; set; }

        /// <summary>
        /// Content
        /// </summary>
        [MaxLength(255, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Content { get; set; }

        public StudentReview? StudentReview { get; set; }
        public Guid StudentReviewId { get; set; }
    }
}
