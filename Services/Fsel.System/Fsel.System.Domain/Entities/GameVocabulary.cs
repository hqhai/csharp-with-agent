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
        [Range(1000000, 9999999, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public int? Code { get; set; }
        [Required]
        [MaxLength(500, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Key { get; set; }
        public EnumGameCefrLevel CefrLevel { get; set; }
        public EnumGameCourseLevel CourseLevel { get; set; }
        public EnumUnitOrder UnitOrder { get; set; }
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

        public string? UsEquivalent { get; set; }
        public EnumPartSpeech? PartSpeech { get; set; }
        public string? Definition { get; set; }
        public string? Hint { get; set; }
        public string? ExampleSentence { get; set; }
        public string? ImagePath { get; set; }
        public string? AudioPath { get; set; }
        public string? Synonym { get; set; }
        public string? Antonym { get; set; }
        public string? PhoneticTranscription { get; set; }
        public Guid? GameCenterId { get; set; }
        public Guid? WordCategoryId { get; set; }
        public GameCenter? GameCenter { get; set; }
    }
}
