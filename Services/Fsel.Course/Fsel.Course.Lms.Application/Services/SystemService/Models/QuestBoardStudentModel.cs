// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.SystemService.Models
{
    using System;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class QuestBoardStudentModel : BaseModel
    {
        public EnumQuestBoardStudentStatus Status { get; set; }

        public int AchievedPoints { get; set; }

        public Guid QuestBoardId { get; set; }

        public Guid StudentId { get; set; }

        public Guid? ObjectId { get; set; }
    }
}
