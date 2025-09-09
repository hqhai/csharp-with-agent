// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Attributes;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;

    public class CurriculumConfig : Entity
    {
        /// <summary>
        /// Tên giáo trình
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MinLength(1, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        [MaxLength(100, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        [RegexValid(Regex = "^[A-Za-z0-9,-]{1,100}$", ErrorMessage = nameof(EnumSystemErrorCode.InValidFormat))]
        public string? CurriculumName { get; set; }

        /// <summary>
        /// Id giáo trình gốc
        /// </summary>
        public Guid? CourseId { get; set; }

        /// <summary>
        /// Ngày bắt đầu
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Ngày kết thúc
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public DateTime? EndDate { get; set; }
        public EnumCurriculumStatus CurriculumStatus { get; set; }
        public ICollection<CurriculumStudent> CurriculumStudent { get; set; } = new List<CurriculumStudent>();
    }
}
