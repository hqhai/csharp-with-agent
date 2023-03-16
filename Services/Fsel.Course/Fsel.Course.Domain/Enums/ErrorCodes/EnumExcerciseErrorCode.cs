namespace Fsel.Course.Domain.Enums.ErrorCodes
{
    public enum EnumExcerciseErrorCode
    {
        /// <summary>
        /// Excercise does not exist
        /// </summary>
        E01V,

        /// <summary>
        /// Excercise null
        /// </summary>
        E03V,

        /// <summary>
        /// Name cannot be empty
        /// </summary>
        E01C,

        /// <summary>
        /// Name limited to 250 characters
        /// </summary>
        E02C,

        /// <summary>
        /// MediaPost limited to 1000 characters
        /// </summary>
        E03C
    }
}
