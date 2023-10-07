// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Domain.Models.EntityModels
{
    using System;
    using Fsel.Core.Base.BaseModels;

    public class QuestBankModel : BaseModel
    {
        public Guid GameVocabularyId { get; set; }

        public bool IsActive { get; set; }
    }
}
