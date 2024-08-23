// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Enums.ErrorCodes
{
    public enum EnumChangeLevelErrorCode
    {
        /// <summary>
        /// The number of changes has expired
        /// </summary>
        ChangesExpired,

        /// <summary>
        /// The number of retakes has expired
        /// </summary>
        RetakesExpired,

        /// <summary>
        /// Not at the same level as current
        /// </summary>
        RetakeNotSameLevel,

        /// <summary>
        /// was not selected because he had studied up to unit 3
        /// </summary>
        StudiedUpToUnit3,

        /// <summary>
        /// level is incorrect
        /// </summary>
        LevelIsIncorrect,

        /// <summary>
        /// Do not change the current key
        /// </summary>
        DoNotChangeCurrentKey
    }
}
