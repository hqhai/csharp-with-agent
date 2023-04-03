// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.ComponentModel.DataAnnotations;
    using Fsel.Core.Entities;
    using Fsel.Course.Domain.Enums;

    public class QuestionForm : Entity
    {
        /// <summary>
        /// Tên QuestionForm
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Loại câu hỏi
        /// </summary>
        public EnumQuestionType Type { get; set; }

        /// <summary>
        /// Config
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public string? ConfigStr { get; set; }

        [NotMapped]
        public object? Config
        {
            get { return ConvertHelper.Deserialize<object>(ConfigStr); }
            set { ConfigStr = ConvertHelper.Serialize(value); }
        }
    }
}
