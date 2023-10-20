// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;

    public class ZMatterModel : BaseModel
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Usage { get; set; }
        public string? Code { get; set; }
        public string? FilePath { get; set; }
        public bool IsActive { get; set; }
    }
}
