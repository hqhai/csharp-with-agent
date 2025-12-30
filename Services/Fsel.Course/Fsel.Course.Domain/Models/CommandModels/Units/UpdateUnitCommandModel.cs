// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base.BaseModels;

namespace Fsel.Course.Domain.Models.CommandModels.Units
{
    using Entities;
    using Enums;

    public class UpdateUnitCommandModel : BaseCommandModel
    {
        public string? Name { get; set; }
        public string? Code { get; set; }
        public IList<Module>? Modules { get; set; }
        public IList<HighlightRange>? HighlightRanges { get; set; }
        public IList<HighlightRange>? ProgressSpeedometerRanges { get; set; }
        public Guid ProgramId { get; set; }
        public Guid LevelId { get; set; }
    }

    public class Module
    {
        public Guid Id { get; set; }

        public EnumUnitConfigType ModuleType { get; set; }

        public double Percent { get; set; }

        public int OpenOrder { get; set; }
    }
}
