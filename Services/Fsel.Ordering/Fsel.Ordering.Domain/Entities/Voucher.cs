// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics.Eventing.Reader;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Fsel.Common.Enums.ErrorCodes;
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
        public EnumCustomerType CustomerType { get; set; }
        public EnumCourseLevel CourseLevel { get; set; }
        public ICollection<VoucherPackage> VoucherPackages { get; set; } = new List<VoucherPackage>();
    }
}
