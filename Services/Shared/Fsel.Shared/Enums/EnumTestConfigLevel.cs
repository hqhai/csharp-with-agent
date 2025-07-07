// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Enums
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public enum EnumTestConfigLevel
    {
        [Description("Think Starter")]
        A1,

        [Description("Think 1")]
        A2,

        [Description("Think 2")]
        B1,

        [Description("Think 3")]
        B1Plus,

        [Description("Think 4")]
        B2,

        [Description("Think 5")]
        C1,

        [Description("Adult Foundation 1")]
        AF1,

        [Description("Adult Foundation 2")]
        AF2,

        [Description("Adult Foundation 3")]
        AF3
    }
}
