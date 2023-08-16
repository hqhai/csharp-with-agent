// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using global::System;

    public class QuestBoardTaskModel : BaseModel
    {
        public DateTime ImplementDate { get; set; }
        public Guid? DependentTaskId { get; set; }
        public Guid QuestBoardId { get; set; }
    }
}
