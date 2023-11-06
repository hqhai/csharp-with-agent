// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.EntityModels
{
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.Entities;
    using global::System;

    public class QuestBoardStudentModel : BaseModel
    {
        public EnumQuestBoardStudentStatus Status { get; set; }

        public float AchievedPoints { get; set; }

        public QuestBoard? QuestBoard { get; set; }

        public Guid QuestBoardId { get; set; }

        public Guid StudentId { get; set; }

        public Guid? ObjectId { get; set; }
    }
}
