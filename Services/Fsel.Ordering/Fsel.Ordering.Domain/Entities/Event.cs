// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Entities
{
    using Fsel.Common.Enums.ErrorCodes;
    using System.ComponentModel.DataAnnotations;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.Interfaces;

    public class Event : Entity, IMultiLingualObject<EventTranslation>
    {
        /// <summary>
        /// Code Event
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(6, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Code { get; set; }

        /// <summary>
        /// Name Event
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Name { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Sự kiện mặc định
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// Mô tả
        /// </summary>
        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Description { get; set; }

        /// <summary>
        /// ảnh banner
        /// </summary>
        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? ImagePathsStr { get; set; }

        [NotMapped]
        public IList<string>? ImagePaths
        {
            get { return ConvertHelper.Deserialize<IList<string>>(ImagePathsStr); }
            set { ImagePathsStr = ConvertHelper.Serialize(value); }
        }

        #region không dùng field này

        /// <summary>
        /// Trạng thái
        /// </summary>
        public EnumEventPackageStatus Status { get; set; }

        #endregion không dùng field này

        public ICollection<PackageEvent> PackageEvents { get; set; } = new List<PackageEvent>();
        public ICollection<EventTranslation> Translations { get; set; } = new List<EventTranslation>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }

    public class EventTranslation : Entity, ITranslationObject
    {
        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public string? Description { get; set; }

        public Guid EventId { get; set; }

        public Event? Event { get; set; }

        public string? Language { get; set; }
    }
}
