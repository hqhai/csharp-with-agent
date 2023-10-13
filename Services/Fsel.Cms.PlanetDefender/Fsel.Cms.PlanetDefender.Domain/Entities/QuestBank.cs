// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Domain.Entities
{
    using System;
    using Fsel.Core.Entities;

    public class QuestBank : Entity
    {
        public Guid GameVocabularyId { get; set; }

        public bool IsActive { get; set; }
    }
}
