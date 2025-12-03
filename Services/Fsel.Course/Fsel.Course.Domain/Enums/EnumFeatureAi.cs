// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Enums
{
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    public enum EnumFeatureMultiple
    {
        [Description("Unit")]
        Unit,
        [Description("Lesson")]
        Lesson,
        [Description("Test")]
        Test,
    }
}
