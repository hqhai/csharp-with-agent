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
        /// Teacher Does not exits
        /// </summary>
        TeacherNotExits,

        /// <summary>
        /// StartDate already exists
        /// </summary>
        StartDateAlreadyExists,

        /// <summary>
        /// EndDate already exists
        /// </summary>
        EndDateAlreadyExists,
    }
}
