// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.ArchiveModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class PlacementTestArchiveModel : BaseModel
    {
        public string? Name { get; set; }
        public EnumPlacementTestLevel Level { get; set; }
    }
}
