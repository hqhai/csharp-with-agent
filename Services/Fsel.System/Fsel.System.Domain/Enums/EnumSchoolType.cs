// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Enums
{
    using global::System.ComponentModel;

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
