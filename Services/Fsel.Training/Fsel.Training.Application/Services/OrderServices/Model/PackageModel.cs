// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Application.Services.OrderServices.Model
{
    using Fsel.Shared.Enums;

    public class PackageModel
    {
        public Guid Id { get; set; }
        public EnumPackageCode? Code { get; set; }
    }
}
