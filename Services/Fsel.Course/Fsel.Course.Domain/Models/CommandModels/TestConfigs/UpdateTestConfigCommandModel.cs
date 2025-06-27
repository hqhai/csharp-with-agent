// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.TestConfig
{
    using Fsel.Core.Base.BaseModels;

    public class UpdateTestConfigCommandModel : BaseCommandModel
    {
        public string? Name { get; set; }
        public double Version { get; set; }
    }
}
