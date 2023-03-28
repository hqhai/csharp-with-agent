// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Enums.ErrorCodes
{
    public enum EnumTeacherErrorCode
    {
        /// <summary>
        /// Passport Path limited to 1000 characters
        /// </summary>
        TE01C,

        /// <summary>
        /// University Degree Path limited to 1000 characters
        /// </summary>
        TE02C,

        /// <summary>
        /// Certification Path limited to 1000 characters
        /// </summary>
        TE03C,

        /// <summary>
        /// Police Clearance Path limited to 1000 characters
        /// </summary>
        TE04C,

        /// <summary>
        /// teacher does not exist
        /// </summary>
        TeacherIdDoesNotExitst,

        /// <summary>
        /// teachers does not exist
        /// </summary>
        TeachersDoesNotExitst,
    }
}
