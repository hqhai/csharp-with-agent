namespace Fsel.Course.Domain.Enums.ErrorCodes
{
    public enum EnumUnitLessonErrorCode
    {
        /// <summary>
        /// Lesson Unit does not exist
        /// </summary>
        LU01V,

        /// <summary>
        /// Lesson Unit was used
        /// </summary>
        LU02V,

        /// <summary>
        /// Lesson Unit not is correct
        /// </summary>
        LU03V,

        /// <summary>
        /// Name cannot be empty
        /// </summary>
        LU01C,

        /// <summary>
        /// Name limited to 250 characters
        /// </summary>
        LU02C,

        /// <summary>
        /// Instruction Content limited to 1000 characters
        /// </summary>
        LU03C,
    }
}