namespace Fsel.Course.Domain.Enums.ErrorCodes
{
    public enum EnumUnitSkillMockTestErrorCode
    {
        /// <summary>
        /// Unit Skill Mock Test does not exist
        /// </summary>
        USMT01V,

        /// <summary>
        /// Unit Skill Mock Test was used
        /// </summary>
        USMT02V,

        /// <summary>
        /// Unit Skill Mock Test not is correct
        /// </summary>
        USMT03V,

        /// <summary>
        /// Name cannot be empty
        /// </summary>
        USMT01C,

        /// <summary>
        /// Name limited to 250 characters
        /// </summary>
        USMT02C,
    }
}
