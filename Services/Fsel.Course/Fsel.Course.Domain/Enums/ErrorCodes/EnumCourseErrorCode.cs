namespace Fsel.Course.Domain.Enums.ErrorCodes
{
    public enum EnumCourseErrorCode
    {
        /// <summary>
        /// Course does not exist
        /// </summary>
        C01V,

        /// <summary>
        /// Course have been use
        /// </summary>
        C02V,

        /// <summary>
        /// Course is not in a new state
        /// </summary>
        C03V,

        /// <summary>
        /// Name cannot be empty
        /// </summary>
        C01C,

        /// <summary>
        /// Name limited to 250 characters
        /// </summary>
        C02C,

        /// <summary>
        /// NumberOfUnits of course >1
        /// </summary>
        C03C,

        /// <summary>
        /// NumberOfLessons of course >1
        /// </summary>
        C04C
    }
}
