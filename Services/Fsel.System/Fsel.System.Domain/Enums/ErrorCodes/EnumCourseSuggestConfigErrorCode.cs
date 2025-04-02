// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Enums.ErrorCodes
{
    public enum EnumCourseSuggestConfigErrorCode
    {
        RecordWithSameTypeAndLevelHasOverlappingAges,

        AgeMustBeBetweenZeroAndOneHundredFifty,

        ToAgeNotThanFromAge,

        YouChoseTheWrongLevel
    }
}
