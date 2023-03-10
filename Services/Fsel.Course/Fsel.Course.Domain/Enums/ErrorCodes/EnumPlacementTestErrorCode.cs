namespace Fsel.Course.Domain.Enums.ErrorCodes
{
    public enum EnumPlacementTestErrorCode
    {
        /// <summary>
        /// Placement Test does not exist
        /// </summary>
        PT01V,

        /// <summary>
        /// Placement Test was used
        /// </summary>
        PT02V,

        /// <summary>
        /// Placement Test not is correct
        /// </summary>
        PT03V,

        /// <summary>
        /// Name cannot be empty
        /// </summary>
        PT01C,

        /// <summary>
        /// Name limited to 250 characters
        /// </summary>
        PT02C,

        /// <summary>
        /// Instruction Content limited to 1000 characters
        /// </summary>
        PT03C,
    }
}