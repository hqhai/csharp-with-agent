namespace Fsel.Course.Domain.Enums.ErrorCodes
{
    public enum EnumMockTestErrorCode
    {
        /// <summary>
        /// Mock Final Test does not exist
        /// </summary>
        MT01V,

        /// <summary>
        /// Mock Final Test  was used
        /// </summary>
        MT02V,

        /// <summary>
        /// Mock Final Test  not is correct
        /// </summary>
        MT03V,

        /// <summary>
        /// Name cannot be empty
        /// </summary>
        MT01C,

        /// <summary>
        /// Name limited to 250 characters
        /// </summary>
        MT02C,
    }
}