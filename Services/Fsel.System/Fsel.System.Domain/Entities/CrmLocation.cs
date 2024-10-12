// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities
{
    using global::System.ComponentModel;

    public class CrmLocation
    {
        public int Id { get; set; }
        public Guid GlobalId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public EnumCrmLocationTypeName? TypeName { get; set; }
        public string? LocalIdPath { get; set; }
        public string? IdPath { get; set; }
        public string? LongPath { get; set; }
        public string? ShortPath { get; set; }
        public int? Level { get; set; }
        public int? ParentId { get; set; }
        public string? LocalId { get; set; }
        public EnumCrmLocationTypeLevel? TypeLevel { get; set; }
    }

    public enum EnumCrmLocationTypeName
    {
        Site,
        School
    }

    public enum EnumCrmLocationTypeLevel
    {
        /// <summary>
        /// Primary Schools - Tiểu học
        /// </summary>
        [Description("Primary Schools")]
        PrimarySchoolsType0 = 0,

        /// <summary>
        /// Primary Schools - Tiểu học
        /// </summary>
        [Description("Primary Schools")]
        PrimarySchools = 1,

        /// <summary>
        /// SecondarySchools - Trung học cơ sở
        /// </summary>
        [Description("Secondary Schools")]
        SecondarySchools = 2,

        /// <summary>
        /// High schools - Trung học phổ thông
        /// </summary>
        [Description("High Schools")]
        HighSchools = 3,

        /// <summary>
        /// Universities/Colleges - Đại học / Cao đẳng
        /// </summary>
        [Description("Universities/Colleges")]
        Universities = 5,
    }
}
