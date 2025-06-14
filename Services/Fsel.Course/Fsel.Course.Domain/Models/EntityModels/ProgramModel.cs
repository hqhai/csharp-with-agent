// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Models.EntityModels.FlowModels;
    using Fsel.Shared.Enums;

    public class ProgramModel : BaseModel
    {
        public string? Name { get; set; }

        public string? Code { get; set; }

        public string? Description { get; set; }

        public EnumTypeCategory Type { get; set; }

        public EnumStatus Status { get; set; }

        public IList<LevelModel>? Levels { get; set; }
        public IList<FlowModel>? Flows { get; set; }
    }
}
