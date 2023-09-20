// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.OrderServices.Model
{
    using Fsel.Ordering.Domain.Enums;

    public class PackageModel
    {
        public Guid Id { get; set; }
        public EnumPackageCode? Code { get; set; }
    }
}
