// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Entities;

    public class Package : Entity
    {
        /// <summary>
        /// Code
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Code { get; set; }

        /// <summary>
        /// Giá Khóa Học
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public double Price { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public string? DescriptionStr { get; set; }

        [NotMapped]
        public IList<string>? Description
        {
            get { return ConvertHelper.Deserialize<IList<string>>(DescriptionStr); }
            set { DescriptionStr = ConvertHelper.Serialize(value); }
        }

        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
