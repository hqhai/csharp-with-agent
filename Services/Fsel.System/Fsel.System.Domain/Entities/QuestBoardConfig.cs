// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities
{
    using Fsel.Common.Enums;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;

    public class QuestBoardConfig : Entity
    {
        public EnumQuestBoardType Type { get; set; }
        public EnumQuestBoardCategory Category { get; set; }

        public int MaxPoints { get; set; }

        public EnumFilterOperator Operator { get; set; }

        public EnumDisplayType DisplayType { get; set; }

        public string? TaskPageUrl { get; set; }
    }
}
