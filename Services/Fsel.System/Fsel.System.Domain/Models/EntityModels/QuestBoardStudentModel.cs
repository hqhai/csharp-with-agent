// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.EntityModels
{
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;
    using Fsel.System.Domain.Entities;
    using global::System;
    using global::System.Collections.Generic;
    using global::System.ComponentModel.DataAnnotations;
    using global::System.Linq;
    using global::System.Text;
    using global::System.Threading.Tasks;

    public class QuestBoardStudentModel : BaseModel
    {
        public EnumQuestBoardStudentStatus Status { get; set; }

        public int AchievedPoints { get; set; }

        public QuestBoard? QuestBoard { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid QuestBoardId { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid StudentId { get; set; }

        public Guid? ObjectId { get; set; }
    }
}
