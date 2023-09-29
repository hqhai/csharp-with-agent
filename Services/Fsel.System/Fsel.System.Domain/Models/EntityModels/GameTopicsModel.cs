// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.EntityModels
{
    using Fsel.Shared.Enums;
    using global::System.Collections.Generic;

    public class GameTopicsModel
    {
        public EnumUnitNumber UnitOrder { get; set; }

        public IList<GameTopicModel>? GameTopics { get; set; }
    }
}
