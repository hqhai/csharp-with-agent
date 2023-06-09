// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Entities;
    using Fsel.Ordering.Domain.Entities.PackageConfigs;
    using Fsel.Ordering.Domain.Enums;

    public class Package : Entity
    {
        /// <summary>
        /// Code
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public EnumPackageCode? Code { get; set; }

        /// <summary>
        /// Giá Khóa Học
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = nameof(EnumSystemErrorCode.Min))]
        public decimal Price { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public string? DescriptionStr { get; set; }

        [NotMapped]
        public IList<PackageConfig>? Description
        {
            get { return ConvertHelper.Deserialize<IList<PackageConfig>>(DescriptionStr); }
            set { DescriptionStr = ConvertHelper.Serialize(value); }
        }

        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
