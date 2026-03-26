// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Master.Domain.Models.EntityModels
{
    using System.Collections.Generic;

    public class ProvinceLevelsModel
    {
        public Guid UnitId { get; set; }
        public string? UnitName { get; set; }
        public IList<ProvinceLevelModels>? Levels { get; set; }
    }

    public class ProvinceLevelModels
    {
        public Guid LevelId { get; set; }
        public string? LevelName { get; set; }
        public int? DisplayOrder { get; set; }
        public string? ProgramName { get; set; }
        public int? CountStudent { get; set; }
    }
}
