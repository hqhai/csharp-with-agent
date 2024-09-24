// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities.QuestBoards
{
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;
    using global::System.ComponentModel.DataAnnotations;

    public class QuestBoard : Entity
    {
        /// <summary>
        /// Loại nhiệm vụ
        /// </summary>
        public EnumQuestBoardType Type { get; set; }

        /// <summary>
        /// Tên nhiệm vụ
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Name { get; set; }

        /// <summary>
        /// Mô tả chi tiết nhiệm vụ
        /// </summary>
        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Description { get; set; }

        /// <summary>
        /// Link ảnh
        /// </summary>
        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? ImagePath { get; set; }

        /// <summary>
        /// Danh mục nhiệm vụ
        /// </summary>
        public EnumQuestBoardCategory Category { get; set; }

        /// <summary>
        /// Giá trị mục tiêu
        /// </summary>
        public int TargetValue { get; set; }

        /// <summary>
        /// Coin nhận được khi hoàn thành
        /// </summary>
        public int Token { get; set; }

        /// <summary>
        /// Năng lượng nhận được khi hoàn thành
        /// </summary>
        public int? Energy { get; set; }

        /// <summary>
        /// Lặp lại theo
        /// </summary>
        public EnumRepeatType? RepeatType { get; set; }

        /// <summary>
        /// Bật tắt nhiệm vụ
        /// </summary>
        public bool IsActive { get; set; }

        public ICollection<QuestBoardStudent> QuestBoardStudents { get; set; } = new List<QuestBoardStudent>();
    }
}
