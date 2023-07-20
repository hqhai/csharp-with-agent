// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Enums
{
    using System.ComponentModel;

    public enum EnumReviewQuestionType
    {
        [Description("INTERFACE_FRIENDLY")]
        INTERFACEFRIENDLY,

        [Description("FEATURES_ACCESSIBLE")]
        FEATURESACCESSIBLE,

        [Description("PROCESSING_PLATFORM")]
        PROCESSINGPLATFORM,

        [Description("SATISFIED_TEACHER")]
        SATISFIEDTEACHER,

        [Description("LEVEL_CHALLENGE")]
        LEVELCHALLENGE,

        [Description("COURSE_RELEVANT")]
        COURSERELEVANT,

        [Description("GAINED_KNOWLEDGE")]
        GAINEDKNOWLEDGE,

        [Description("QUALITY_PICTURE")]
        QUALITYPICTURE
    }
}
