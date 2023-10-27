// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.FinalTests
{
    using System.Collections.Generic;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Models.CommandModels.SectionGroups;
    using Fsel.Shared.Enums;

    public class UpdateFinalTestCommandModel : BaseCommandModel
    {
        public string? Name { get; set; }

        public bool IsActive { get; set; }

        public double ExecutionTime { get; set; }

        public EnumFinalTestLevel FinalTestLevel { get; set; }

        public IList<CreateSectionGroupCommandModel>? SectionGroups { get; set; }
    }
}
