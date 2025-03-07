// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities
{
    using Fsel.Shared.Enums;
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
        public EnumCrmLocationLevel? Level { get; set; }
        public string? LocalId { get; set; }
        public EnumCrmLocationTypeLevel? TypeLevel { get; set; }
        public int? ParentId { get; set; }
        public CrmLocation? Parent { get; set; }
    }

    public enum EnumCrmLocationTypeName
    {
        Site,
        School
    }

    public enum EnumCrmLocationLevel
    {
        Country,
        Zone,
        Province,
        District,
        School
    }
}
