// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities
{
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;
    using global::System.ComponentModel.DataAnnotations;

    public class QuestBoardStudent : Entity
    {
        public EnumQuestBoardStudentStatus Status { get; set; }

        public float AchievedPoints { get; set; }

        public QuestBoard? QuestBoard { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid QuestBoardId { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid StudentId { get; set; }

        public Guid? ObjectId { get; set; }
    }
}
