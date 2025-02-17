// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class DisplayOrderConfigModel : BaseModel
    {
        public int DisplayOrder { get; set; }

        public EnumDisplayOrder Name { get; set; }

        public bool Status { get; set; }
    }
}
