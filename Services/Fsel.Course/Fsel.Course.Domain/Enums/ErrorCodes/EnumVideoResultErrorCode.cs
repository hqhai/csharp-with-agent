// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Enums.ErrorCodes
{
    public enum EnumVideoResultErrorCode
    {
        /// <summary>
        /// You didn't do enough questions
        /// </summary>
        NotEnoughQuestions,

        /// <summary>
        /// VideoTimeCode Is Not Completed
        /// </summary>
        VideoTimeCodeNotCompleted,

        /// <summary>
        /// Select Video TimeCode first
        /// </summary>
        VideoTimeCodeNotFirst,

        /// <summary>
        /// Coin is displayed
        /// </summary>
        CoinDisplayed
    }
}
