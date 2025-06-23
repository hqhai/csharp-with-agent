// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.TestConfig
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;

    public class TestLayout : Entity
    {
        public EnumTestLayoutName Name { get; set; }
        public double? TotalScore { get; set; }
        public string? ExcutionTime { get; set; }
    }
}
