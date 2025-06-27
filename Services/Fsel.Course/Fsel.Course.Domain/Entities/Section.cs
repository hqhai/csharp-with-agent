// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;

    public class Section : Entity
    {
        /// <summary>
        /// Tên Section
        /// </summary>
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Name { get; set; }

        /// <summary>
        /// Media Post
        /// </summary>
        public string? MediaPost { get; set; }

        [Range(0, 10000_0000, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public int TargetWord { get; set; }

        /// <summary>
        /// Video File Path
        /// </summary>
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? VideoFilePath { get; set; }

        /// <summary>
        /// Sub File Path
        /// </summary>
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? SubFilePath { get; set; }

        public int DisplayOrder { get; set; }

        public SectionGroup? SectionGroup { get; set; }
        public Guid SectionGroupId { get; set; }

        public ICollection<SectionPart> SectionParts { get; set; } = new List<SectionPart>();
        public ICollection<SectionTimeCode> SectionTimeCodes { get; set; } = new List<SectionTimeCode>();
        public ICollection<SectionQuestion> SectionQuestions { get; set; } = new List<SectionQuestion>();
        public ICollection<ExtraPracticeAnswer> ExtraPracticeAnswers { get; set; } = new List<ExtraPracticeAnswer>();
        public ICollection<MockTestAnswer> MockTestAnswers { get; set; } = new List<MockTestAnswer>();
        public ICollection<MockTestAISetting>? MockTestAISettings { get; set; }
    }
}
