// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Domain.Models.CommandModel.GameAnswers
{
    using System;

    public class AnswerTheQuestionCommandModel
    {
        public Guid GameVocabularyId { get; set; }
        public Guid GameVocabularyTypeId { get; set; }
        public Guid GameHistoryId { get; set; }
        public string? Answer { get; set; }
    }
}
