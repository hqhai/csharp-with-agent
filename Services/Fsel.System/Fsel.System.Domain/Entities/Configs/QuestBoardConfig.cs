// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities.Configs
{
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;

    public class QuestBoardConfig : Entity
    {
        public EnumQuestBoardType Type { get; set; }
        public EnumQuestBoardCategory Category { get; set; }
    }
}
