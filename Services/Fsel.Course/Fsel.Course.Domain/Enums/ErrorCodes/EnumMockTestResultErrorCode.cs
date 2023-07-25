// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Enums.ErrorCodes
{
    public enum EnumMockTestResultErrorCode
    {
        /// <summary>
        /// Mock Test Result does not exist
        /// </summary>
        MockTestResultNotExist,

        /// <summary>
        /// Mock Test Result Done
        /// </summary>
        MockTestResultDone,

        /// <summary>
        /// MockTestScoresNull
        /// </summary>
        MockTestScoresNull,

        /// <summary>
        /// ScoreMustLessThan9
        /// </summary>
        ScoreMustLessThan9,

        /// <summary>
        /// SectionGroups Not Exist
        /// </summary>
        SectionGroupsNotExist
    }
}
