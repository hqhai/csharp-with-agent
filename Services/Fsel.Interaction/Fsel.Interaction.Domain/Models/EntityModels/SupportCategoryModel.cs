// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.EntityModels
{
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.BaseModels;

    public class SupportCategoryModel : BaseModel
    {
        public string? Title { get; set; }

        public string? Name { get; set; }

        private string? _iconPath;
        public string? IconPath
        {
            set { _iconPath = value; }
            get { return _iconPath.AddS3BaseUrl(); }
        }

        public bool IsActive { get; set; }
        public int NumberOfQuestion { get; set; }
    }
}
