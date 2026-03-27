// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Master.Domain.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Shared.Enums;

    [Table("Dim_Location")]
    public class DimLocation
    {
        [Key]
        public Guid LocationId { get; set; }
        public int? LocationCrmId { get; set; }
        public Guid? GlobalId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? TypeName { get; set; }
        public string? LocalIdPath { get; set; }
        public string? IdPath { get; set; }
        public string? LongPath { get; set; }
        public string? ShortPath { get; set; }
        public EnumLocationType? Level { get; set; }
        public string? LocalId { get; set; }
        public EnumCrmLocationTypeLevel? TypeLevel { get; set; }
        public int? ParentId { get; set; }
        public int? ElevationOfTerrain { get; set; }
        public int? ElevationOfRefHeight { get; set; }
    }
}
