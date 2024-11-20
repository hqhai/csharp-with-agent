// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Enums
{
    using System.ComponentModel;

    public enum EnumSchoolType
    {
        [Description("Công lập")]
        Public,

        [Description("Bán công")]
        SemiPublic,

        [Description("Tư thục")]
        Private,

        [Description("Ngoài công lập")]
        NonPublic
    }
}
