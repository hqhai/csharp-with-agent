namespace Fsel.Course.Domain.Enums.ErrorCodes
{
    public enum EnumClassForumErrorCode
    {
        /// <summary>
        /// ClassForum does not exist
        /// </summary>
        CF01V,

        /// <summary>
        /// ClassForum have been use
        /// </summary>
        CF02V,

        /// <summary>
        /// ClassForum null
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
        /// Instruction Content limited to 1000 characters
        /// </summary>
        CF03C,

        /// <summary>
        /// Number of course >1
        /// </summary>
        CF04C
    }
}