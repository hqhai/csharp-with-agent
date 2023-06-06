// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Enums
{
    using System.ComponentModel;

    public enum EnumExtraPracticeType
    {
        [Description("Video embed")]
        VideoEmbed,

        [Description("Interactive Video")]
        InteractiveVideo,

        [Description("Book")]
        Book,

        [Description("Exercise")]
        Exercise,

        [Description("Articles")]
        Articles
    }
}
