// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Domain.Models.CommandModel
{
    using System;
    using Fsel.Core.Base.BaseModels;

    public class CreateQuestBankCommandModel : BaseQueryModel
    {
        public Guid GameVocabulary { get; set; }

        public bool IsActive { get; set; }
    }
}
