// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;

    public class BaseAnswer : Entity
    {
        [NotMapped]
        public override Guid? UpdatedUserId { get; set; }

        [NotMapped]
        public override Guid? DeletedUserId { get; set; }

        [NotMapped]
        public override string? UpdatedFullName { get; set; }

        [NotMapped]
        public override string? DeletedFullName { get; set; }

        [NotMapped]
        public override DateTime? UpdatedDate { get; set; }

        [NotMapped]
        public override DateTime? DeletedDate { get; set; }

        /// <summary>
        /// Câu trả lời
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(11000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public virtual string? AnswerStr { get; set; }

        [NotMapped]
        public virtual object? Answer
        {
            get { return ConvertHelper.Deserialize<object>(AnswerStr); }
            set { AnswerStr = ConvertHelper.Serialize(value); }
        }

        public EnumAnswerStatus Status { get; set; }

        public bool? IsCorrect { get; set; }

        /// <summary>
        /// Số lượng câu trả lời đúng
        /// </summary>
        [Column(TypeName = "smallint")]
        public virtual short CorrectCount { get; set; }
    }
}
