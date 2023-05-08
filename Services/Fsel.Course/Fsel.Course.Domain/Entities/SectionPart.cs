// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using Fsel.Common.Enums.ErrorCodes;
    using System.ComponentModel.DataAnnotations;

    public class SectionPart
    {
        /// <summary>
        /// Tên Part
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? PartName { get; set; }
        public Section? Section { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid SectionId { get; set; }

        public ICollection<SectionQuestion> SectionQuestions { get; set; } = new List<SectionQuestion>();


    }
}
