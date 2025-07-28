// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Enums.ErrorCodes
{
    public enum EnumUnitErrorCode
    {
        /// <summary>
        /// Unit have been use
        /// </summary>
        UnitUsed,

        /// <summary>
        /// Highlight range is invalid (From must be less than To)
        /// </summary>
        InvalidHighlightRange,

        /// <summary>
        /// Highlight ranges must start from 0 and end at 100
        /// </summary>
        HighlightRangeMissingBoundary,

        /// <summary>
        /// Highlight ranges contain invalid values like 0 or 100 in the middle
        /// </summary>
        HighlightRangeInvalidInnerValue,

        /// <summary>
        /// Highlight ranges are not sequential or have gaps/overlap
        /// </summary>
        HighlightRangeOverlapOrGap
    }
}
