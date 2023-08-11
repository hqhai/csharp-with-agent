// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities
{
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;
    using global::System.ComponentModel.DataAnnotations;
    using global::System.ComponentModel.DataAnnotations.Schema;

    public class QuestBoard : Entity
    {
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
        /// Loại nhiệm vụ
        /// </summary>
        public EnumQuestBoardType Type { get; set; }

        /// <summary>
        /// Danh mục nhiệm vụ
        /// </summary>
        public EnumQuestBoardCategory Category { get; set; }

        /// <summary>
        /// Ngày băt đầu
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Ngày kết thúc
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Tổng số sao đạt được
        /// </summary>
        public int NumberOfStars { get; set; }

        /// <summary>
        /// Lặp lại theo
        /// </summary>
        public EnumRepeatType RepeatType { get; set; }

        /// <summary>
        /// Loại Package
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public string? PackageIdsStr { get; set; }

        [NotMapped]
        public IList<Guid>? PackageIds
        {
            get { return ConvertHelper.Deserialize<IList<Guid>>(PackageIdsStr); }
            set { PackageIdsStr = ConvertHelper.Serialize(value); }
        }

        /// <summary>
        /// Yêu cầu bắt buộc
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Bật tắt nhiệm vụ
        /// </summary>
        public bool IsActice { get; set; }

        /// <summary>
        /// Nhiệm vụ phụ thuộc
        /// </summary>
        public Guid? DependentId { get; set; }

        public ICollection<QuestBoardTask> QuestBoardTasks { get; set; } = new List<QuestBoardTask>();
    }
}
