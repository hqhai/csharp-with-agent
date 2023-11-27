// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Domain.Entities
{
    using System;
    using Fsel.Core.Entities;

    public class GameAnswer : Entity
    {
        public string? Answer { get; set; }
        public bool IsCorrect { get; set; }
        public Guid GameVocabularyId { get; set; }
        public Guid GameVocabularyTypeId { get; set; }
        public Guid StudentId { get; set; }
        public Guid GameHistoryId { get; set; }
        public GameHistory? GameHistory { get; set; }
    }
}
