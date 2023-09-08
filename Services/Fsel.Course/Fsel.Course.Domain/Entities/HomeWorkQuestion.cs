// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;
    using System.ComponentModel.DataAnnotations;

    public class HomeWorkQuestion : Entity
    {
        public Question? Question { get; set; }

        public HomeWork? HomeWork { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid HomeWorkId { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid QuestionId { get; set; }

        public ICollection<HomeWorkAnswer> HomeWorkAnswers { get; set; } = new List<HomeWorkAnswer>();
    }
}
