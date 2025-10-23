// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Enums.ErrorCodes
{
    public enum EnumStudentErrorCode
    {
        /// <summary>
        /// Class with more than 12 students
        /// </summary>
        ClassMoreThan12Students,

        /// <summary>
        /// received tokens
        /// </summary>
        UserReceivedTokens,

        /// <summary>
        /// User Not Established
        /// </summary>
        UserNotEstablished,

        /// <summary>
        /// User Not EnoughTime
        /// </summary>
        UserNotEnoughTime,

        /// <summary>
        /// Student Already Another Event
        /// </summary>
        StudentAlreadyAnotherEvent,

        /// <summary>
        /// Event Not Matching With Current Event
        /// </summary>
        EventNotMatchingWithCurrentEvent,

        /// <summary>
        /// School Not Include Event
        /// </summary>
        SchoolNotIncludeEvent,



        /// <summary>
        /// Student hasn't not expired Date
        /// </summary>
        StudentHasNotExpiredDate,

        /// <summary>
        /// User Sender Setting Not Null
        /// </summary>
        UserSenderSettingNotNull
    }
}
