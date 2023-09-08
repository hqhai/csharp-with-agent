// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Enums.ErrorCodes
{
    public enum EnumTeacherFreeDateErrorCode
    {
        /// <summary>
        /// StartDate is not bigger than EndDate
        /// </summary>
        StartDateNotBiggerThanEndDate,

        /// <summary>
        /// StartDate Bigger Than DateNow
        /// </summary>
        StartDateBiggerThanDateNow,
    }
}
