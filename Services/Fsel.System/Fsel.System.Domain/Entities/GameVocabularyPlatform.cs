// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities
{
    using Fsel.Core.Entities;
    using global::System;

    public class GameVocabularyPlatform : Entity
    {
        public Guid PlatformId { get; set; }
        public Guid GameVocabularyId { get; set; }
        public GameVocabulary? GameVocabulary { get; set; }
    }
}
