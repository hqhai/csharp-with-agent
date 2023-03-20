namespace Fsel.Course.Domain.Enums.ErrorCodes
{
    public enum EnumClassForumErrorCode
    {
        /// <summary>
        /// ClassForum null
        /// </summary>
        CF01V,

        /// <summary>
        /// ClassForum not is correct
        /// </summary>
        CF03V,

        /// <summary>
        /// Name cannot be empty
        /// </summary>
        CF01C,

        /// <summary>
        /// Name limited to 250 characters
        /// </summary>
        CF02C,

        /// <summary>
        /// MediaPost Content limited to 1000 characters
        /// </summary>
        CF03C,

        /// <summary>
        /// TaggetWordLimit of course > 0
        /// </summary>
        CF04C,

        /// <summary>
        /// TaggetTimeLimitTicks of course > 0
        /// </summary>
        CF05C
    }
}
