// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Enums.ErrorCodes
{
    public enum EnumLessonErrorCode
    {
        /// <summary>
        /// Lesson was used
        /// </summary>
        LessonUsed,

        DocumentNotNull,

        ClassForumNotNull,

        CodeNotNullOrEmpty,

        CodeAlreadyExist,

        CodeNotValid,

        InstructionContentNotValid,

        InstructionNotValid,

        LessonModuleNotNull,

        VideoIdNotNull,

        HomeWorkIdNotNull,

        StatusLessonNotInActive,

        PercentNotValid,

        AiConfigNotNull,

        OpenOrderOutOfSequence,

        DisplayOrderOutOfSequence,

        NameNotNullOrEmpty,

        NameNotValid,
    }
}
