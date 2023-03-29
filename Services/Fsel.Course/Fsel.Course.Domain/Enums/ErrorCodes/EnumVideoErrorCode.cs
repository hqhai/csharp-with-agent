namespace Fsel.Course.Domain.Enums.ErrorCodes
{
    public enum EnumVideoErrorCode
    {
        /// <summary>
        /// Video does not exist
        /// </summary>
        VideoNotExist,

        /// <summary>
        /// Video was used
        /// </summary>
        VideoUsed,

        /// <summary>
        /// Video not is correct
        /// </summary>
        VideoNotCorrect,

        /// <summary>
        /// TeacherId does not exist
        /// </summary>
        TeacherIdDoesNotExitst,

        /// <summary>
        /// Config is in the wrong format
        /// </summary>
        ConfigIsInTheWrongFormat
    }
}
