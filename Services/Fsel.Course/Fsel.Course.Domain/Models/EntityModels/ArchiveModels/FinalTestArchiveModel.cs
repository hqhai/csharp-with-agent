// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.ArchiveModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class FinalTestArchiveModel : BaseModel
    {
        public string? Name { get; set; }
        public EnumFinalTestLevel FinalTestLevel { get; set; }
    }
}
