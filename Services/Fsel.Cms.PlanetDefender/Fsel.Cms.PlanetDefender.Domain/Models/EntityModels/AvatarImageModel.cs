// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Domain.Models.EntityModels
{
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.BaseModels;

    public class AvatarImageModel : BaseModel
    {
        private string? _filePath;
        public string? FilePath
        {
            set { _filePath = value; }
            get { return _filePath.AddS3BaseUrl(); }
        }

        public int? Level { get; set; }
    }
}
