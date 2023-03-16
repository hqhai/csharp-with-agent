namespace Fsel.Course.Domain.Enums.ErrorCodes
{
    public enum EnumUnitErrorCode
    {
        /// <summary>
        /// Name cannot be empty
        /// </summary>
        U01C,

        /// <summary>
        /// Name limited to 250 characters
        /// </summary>
        U02C,

        /// <summary>
        /// DisplayName cannot be empty
        /// </summary>
        U03C,

        /// <summary>
        /// DisplayName limited to 250 characters
        /// </summary>
        U04C,

        /// <summary>
        /// Unit does not exist
        /// </summary>
        U01V,

        /// <summary>
        /// Unit have been use
        /// </summary>
        U02V,

        /// <summary>
        /// UnitId is not correct
        /// </summary>
        U03V
    }
}
