// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Enums.ErrorCodes
{
    public enum EnumClassForumResultErrorCode
    {
        /// <summary>
        /// ClassForum Is Null
        /// </summary>
        ClassForumResultNull,

        /// <summary>
        /// Class Forum File Is Null
        /// </summary>
        ClassForumResultFileNull,

        /// <summary>
        /// Class Forum not exist
        /// </summary>
        ClassForumResultNotExist,

        /// <summary>
        /// Class Forum result Status not pendding
        /// </summary>
        ClassForumResultStatusNotPendding,

        /// <summary>
        /// Class Forum result Status not pendding For Grading
        /// </summary>
        ClassForumResultStatusNotPendingForGrading,

        /// <summary>
        /// Can not delete in the current status
        /// </summary>
        CanNotDeleteInCurrentStatus,
    }
}
