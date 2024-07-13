// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Entities
{
    using Fsel.Common.Enums.ErrorCodes;
    using System.ComponentModel.DataAnnotations;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;
    using Fsel.Common.Helpers;
    using System.ComponentModel.DataAnnotations.Schema;

    public class PackageEvent : Entity
    {
        /// <summary>
        /// Giá Khóa Học
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public decimal Price { get; set; }

        /// <summary>
        /// Giá trên tháng
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public decimal PriceMonth { get; set; }

        /// <summary>
        /// Ngày cộng thêm
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public int DayBonus { get; set; }

        /// <summary>
        /// Tháng cộng thêm
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public int MonthBonus { get; set; }

        /// <summary>
        /// Gợi ý
        /// </summary>
        [MaxLength(1000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? SuggestStr { get; set; }

        [NotMapped]
        public IList<EnumPackageSuggest>? Suggests
        {
            get { return ConvertHelper.Deserialize<IList<EnumPackageSuggest>?>(SuggestStr); }
            set { SuggestStr = ConvertHelper.Serialize(value); }
        }

        public Guid PackageId { get; set; }
        public Guid EventId { get; set; }
        public Package? Package { get; set; }
        public Event? Event { get; set; }
    }
}
