// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities
{
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;
    using global::System.ComponentModel.DataAnnotations;
    using global::System.ComponentModel.DataAnnotations.Schema;

    public class GameVocabularyType : Entity
    {
        public EnumGameVocabType GameVocabType { get; set; }
        [MaxLength(500, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? QuestionContent { get; set; }
        [NotMapped]
        public IList<string>? AlternateSpelling
        {
            get
            {
                if (!string.IsNullOrEmpty(QuestionContent))
                {
                    return QuestionContent.Split(';').ToList();
                }
                else
                {
                    return null;
                }
            }
        }
        public Guid GameVocabularyId { get; set; }
        public GameVocabulary? GameVocabulary { get; set; }
    }
}
