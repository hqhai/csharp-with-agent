// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Domain.Models.CommandModel
{
    using System.Collections.Generic;

    public class MassUploadQuestBankCommandModel
    {
        public IList<CreateQuestBankCommandModel>? QuestBanks { get; set; }
    }
}
