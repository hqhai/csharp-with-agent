namespace Fsel.Course.Domain.Enums.ErrorCodes
{
    public enum EnumLessonErrorCode
    {
        /// <summary>
        /// Lesson does not exist
        /// </summary>
        LS01V,

        /// <summary>
        /// Homeword not is correct
        /// </summary>
        LS03V,

        /// <summary>
        /// Lesson was used
        /// </summary>
        LS02V,

        /// <summary>
        /// Name cannot be empty
        /// </summary>
        LS01C,

        /// <summary>
        /// Name limited to 250 characters
        /// </summary>
        LS02C,

        /// <summary>
        /// Instruction Content limited to 1000 characters
        /// </summary>
        LS03C,

        /// <summary>
        /// LessonId is not correct
        /// </summary>
        L03V,
    }
}