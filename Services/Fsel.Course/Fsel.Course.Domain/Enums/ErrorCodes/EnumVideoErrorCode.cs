// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Enums.ErrorCodes
{
    public enum EnumVideoErrorCode
    {
        /// <summary>
        /// Videos name is exist
        /// </summary>
        VideoNameIsExist,

        /// <summary>
        /// Video was used
        /// </summary>
        VideoUsed,

        /// <summary>
        /// Config is in the wrong format
        /// </summary>
        ConfigIsInTheWrongFormat,

        /// <summary>
        /// Cannot have both UnitTest and SkillTest at the same time
        /// </summary>
        CanNotUnitTestAndSkillTestAtTheSameTime,

        /// <summary>
        /// Config Question Invalid Format
        /// </summary>
        ConfigInvalidFormat,

        /// <summary>
        /// Ielts only accepts Standalone
        /// </summary>
        IeltsAcceptsStandalone,

        /// <summary>
        /// AdultFoundation not accepts SkillTest
        /// </summary>
        AdultFoundationNotAcceptsSkillTest,

        /// <summary>
        /// The keyword cannot contain the < or > symbols.
        /// </summary>
        InvalidKeywordCharacter,

        /// <summary>
        /// NotEdited
        /// </summary>
        NotEdited,

        /// <summary>
        /// NotDelete
        /// </summary>
        NotDelete,
    }
}
