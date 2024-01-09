// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Ordering.Domain.Models.EntityModels
{
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Ordering.Domain.Enums;
    using Fsel.Shared.Enums;

    public class VoucherModel : BaseModel
    {
        public string? Name { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        private string? _contentFilePath;
        public string? ContentFilePath
        {
            set { _contentFilePath = value; }
            get { return _contentFilePath.AddS3BaseUrl(); }
        }

        public bool IsGlobal { get; set; }
        public bool? IsActive { get; set; }

        public IList<EnumCustomerType>? CustomerTypes { get; set; }
        public IList<EnumCourseLevel>? CourseLevels { get; set; }

        public IList<VoucherPackageModel>? VoucherPackages { get; set; }
    }
}
