// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Enums.ErrorCodes
{
    public enum EnumCourseSuggestConfigErrorCode
    {
        RecordWithSameTypeAndLevelHasOverlappingAges,

        NoReviewTypeForA1Level,

        NoChallengeTypeForIELTSLevel,

        FromAgeNotLessThanZero,

        FromAgeNotGreaterThan150,

        ToAgeNotLessThanZero,

        ToAgeNotGreaterThan150,

        ToAgeNotThanFromAge
    }
}
