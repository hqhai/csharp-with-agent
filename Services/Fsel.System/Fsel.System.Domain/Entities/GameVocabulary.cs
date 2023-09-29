// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities
{
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;
    using global::System.ComponentModel.DataAnnotations;
    using global::System.ComponentModel.DataAnnotations.Schema;

    public class GameVocabulary : Entity
    {
        [Required]
        [MaxLength(500, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Code { get; set; }
        [Required]
        [MaxLength(500, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Key { get; set; }
        public EnumGameCefrLevel CefrLevel { get; set; }
        public EnumGameCourseLevel CourseLevel { get; set; }
        public EnumUnitNumber UnitOrder { get; set; }
        [MaxLength(500, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? AlternateSpellingStr { get; set; }

        [NotMapped]
        public IList<string>? AlternateSpelling
        {
            get
            {
                if (!string.IsNullOrEmpty(AlternateSpellingStr))
                {
                    return AlternateSpellingStr.Split(';').ToList();
                }
                else
                {
                    return null;
                }
            }
        }
        [MaxLength(500, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? UsEquivalent { get; set; }
        public EnumPartSpeech? PartSpeech { get; set; }
        [MaxLength(500, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Definition { get; set; }
        [MaxLength(500, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Hint { get; set; }
        [MaxLength(500, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? ExampleSentence { get; set; }
        [MaxLength(500, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? ImagePath { get; set; }
        [MaxLength(500, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? AudioPath { get; set; }
        [MaxLength(500, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Synonym { get; set; }
        [MaxLength(500, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Antonym { get; set; }
        [MaxLength(500, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? PhoneticTranscription { get; set; }
        public Guid? GameCenterId { get; set; }
        public Guid? WordCategoryId { get; set; }
        public GameCenter? GameCenter { get; set; }
        public GameTopic? GameTopic { get; set; }
    }
}
