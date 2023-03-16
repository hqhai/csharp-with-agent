namespace Fsel.Course.Domain.Enums.ErrorCodes
{
    public enum EnumVideoTimeCodeErrorCode
    {
        /// <summary>
        /// Video Time Code does not exist
        /// </summary>
        VTC01V,

        /// <summary>
        /// Number of ExecutionTime >1
        /// </summary>
        VTC03C,

        /// <summary>
        /// Number of DisplayTime >1
        /// </summary>
        VTC04C,

        /// <summary>
        /// Video Time Code not is correct
        /// </summary>
        VTC03V,
    }
}
