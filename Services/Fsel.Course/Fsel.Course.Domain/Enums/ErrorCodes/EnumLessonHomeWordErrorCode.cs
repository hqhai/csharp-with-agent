namespace Fsel.Course.Domain.Enums.ErrorCodes
{
    public enum EnumeLessonHomeWordErrorCode
    {
        /// <summary>
        /// Lesson HomeWord does not exist
        /// </summary>
        EHW01V,

        /// <summary>
        /// Lesson HomeWord have been use
        /// </summary>
        EHW02V,

        /// <summary>
        /// Lesson HomeWord null
        /// </summary>
        EHW03V,

        /// <summary>
        /// Name cannot be empty
        /// </summary>
        EHW01C,

        /// <summary>
        /// Name limited to 250 characters
        /// </summary>
        EHW02C,

        /// <summary>
        /// Instruction Content limited to 1000 characters
        /// </summary>
        EHW03C,

        /// <summary>
        /// Number of Lesson HomeWord >1
        /// </summary>
        EHW04C
    }
}