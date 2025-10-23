// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Common.Attributes;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;

    public class CurriculumConfig : Entity
    {
        /// <summary>
        /// Tên giáo trình
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MinLength(2, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        [MaxLength(100, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        [RegexValid(Regex = @"^[\p{L}\p{N}\s,-]{2,100}$", ErrorMessage = nameof(EnumSystemErrorCode.InValidFormat))]
        public string? CurriculumName { get; set; }

        /// <summary>
        /// Id giáo trình gốc
        /// </summary>
        public Guid CourseId { get; set; }

        /// <summary>
        /// Id giáo trình clone
        /// </summary>
        public Guid CourseCloneId { get; set; }

        /// <summary>
        /// Ngày bắt đầu
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Ngày kết thúc
        /// </summary>
        public DateTime EndDate { get; set; }

        [NotMapped]
        public EnumCurriculumStatus CurriculumStatus
        {
            get
            {
                var now = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam);
                if (now < StartDate)
                {
                    return EnumCurriculumStatus.NotProgress;
                }
                else if (now > EndDate)
                {
                    return EnumCurriculumStatus.Expired;
                }
                return EnumCurriculumStatus.Progress;
            }
        }

        public Guid SchoolId { get; set; }

        public ICollection<CurriculumStudent> CurriculumStudent { get; set; } = new List<CurriculumStudent>();
        public ICollection<HomeWorkConfig> HomeWorkConfigs { get; set; } = new List<HomeWorkConfig>();
        public ICollection<HomeWorkRetry> HomeWorkRetries { get; set; } = new List<HomeWorkRetry>();
    }
}
