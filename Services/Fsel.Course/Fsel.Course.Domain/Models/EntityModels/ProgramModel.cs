// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Models.EntityModels.FlowModels;
    using Fsel.Shared.Enums;

    public class ProgramModel : BaseModel
    {
        public string? Name { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
        public EnumTypeCategory Type { get; set; }
        public EnumStatus Status { get; set; }
        public bool IsTestDefault { get; set; }
        public bool IsSubjectTestDefault { get; set; }
        public EnumTestMode? TestMode { get; set; }
        public Guid? ParentId { get; set; }
        public IList<LevelModel>? Levels { get; set; }
        public IList<FlowModel>? Flows { get; set; }
        public IList<Guid>? TestOriginalIds { get; set; }
        public IList<TestOriginalModel>? Tests { get; set; }
    }

    public class TestOriginalModel
    {
        public Guid Id { get; set; }
        public string? Name { set; get; }
    }
}
