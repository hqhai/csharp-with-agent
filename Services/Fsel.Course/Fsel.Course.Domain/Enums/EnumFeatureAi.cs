// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Enums
{
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    public enum EnumFeatureMultiple
    {
        [Description("Unit"), Display(Name = "Unit")]
        Unit,
        [Description("Lesson"), Display(Name = "Lesson")]
        Lesson,
        [Description("Test"), Display(Name = "Test")]
        Test,
    }
}
