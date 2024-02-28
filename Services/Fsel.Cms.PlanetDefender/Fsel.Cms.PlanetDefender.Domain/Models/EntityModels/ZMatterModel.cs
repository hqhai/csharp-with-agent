// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Domain.Models.EntityModels
{
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.BaseModels;

    public class ZMatterModel : BaseModel
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Usage { get; set; }
        public string? Code { get; set; }
        private string? _filePath;
        public string? FilePath
        {
            set { _filePath = value; }
            get { return _filePath.AddS3BaseUrl(); }
        }
        public bool IsActive { get; set; }
        public bool IsOwned { get; set; }
    }
}
