// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.EntityModels
{
    using global::System;

    public class QuestBoardTaskModel
    {
        public DateTime ImplementDate { get; set; }
        public Guid? DependentTaskId { get; set; }
        public Guid QuestBoardId { get; set; }
    }
}
