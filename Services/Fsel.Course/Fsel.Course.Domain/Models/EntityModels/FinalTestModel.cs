// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System.Collections.Generic;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class FinalTestModel : BaseModel
    {
        public string? Name { get; set; }
        public bool IsActive { get; set; }
        public double ExecutionTime { get; set; }
        public long TotalQuestion { get; set; }
        public EnumFinalTestLevel FinalTestLevel { get; set; }
        public IList<SectionGroupModel>? SectionGroups { get; set; }
        public FinalTestResultModel? FinalTestResult { get; set; }
    }
}
