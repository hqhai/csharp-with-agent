// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities
{
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;
    using global::System.ComponentModel.DataAnnotations;

    public class QuestBoardTask : Entity
    {
        /// <summary>
        /// Thời gian bắt đầu nhiệm vụ
        /// </summary>
        public DateTime ImplementDate { get; set; }

        public QuestBoard? QuestBoard { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid DependentTaskId { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid QuestBoardId { get; set; }
    }
}
