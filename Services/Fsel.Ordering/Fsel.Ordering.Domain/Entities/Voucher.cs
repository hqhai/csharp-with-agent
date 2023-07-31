// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Entities;
    using Fsel.Ordering.Domain.Enums;
    using Fsel.Shared.Enums;

    public class Voucher : Entity
    {
        /// <summary>
        /// Tên khóa học
        /// </summary>
        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        [MaxLength(250, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Name { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string? ContentFilePath { get; set; }

        public bool IsGlobal { get; set; }

        public bool IsActive { get; set; }

        public string? CustomerTypesStr { get; set; }

        [NotMapped]
        public IList<EnumCustomerType>? CustomerTypes
        {
            get { return ConvertHelper.Deserialize<IList<EnumCustomerType>>(CustomerTypesStr); }
            set { CustomerTypesStr = ConvertHelper.Serialize(value); }
        }

        public string? CourseLevelsStr { get; set; }

        [NotMapped]
        public IList<EnumCourseLevel>? CourseLevels
        {
            get { return ConvertHelper.Deserialize<IList<EnumCourseLevel>>(CourseLevelsStr); }
            set { CourseLevelsStr = ConvertHelper.Serialize(value); }
        }

        public ICollection<VoucherPackage> VoucherPackages { get; set; } = new List<VoucherPackage>();

        public ICollection<UserVoucher> UserVouchers { get; set; } = new List<UserVoucher>();
    }
}
