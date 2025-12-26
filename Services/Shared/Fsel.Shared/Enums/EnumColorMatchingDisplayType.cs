// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Enums
{
    using System.ComponentModel;

    public enum EnumColorMatchingDisplayType
    {
        [Description("Image only - Users select all images that are the same")]
        ImageMultipleChoice = 1,

        [Description("Text only - Users select all words or sentences with similar meaning")]
        TextMultipleChoice = 2,

        [Description("Image + text description - Users select all items that describe the same category")]
        ImageTextMultipleChoice = 3
    }
}