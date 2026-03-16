// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.TestModels
{
    using Fsel.Common.Enums;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class TestModel : BaseModel
    {
        public string? Name { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
        public Guid ProgramId { get; set; }
        public Guid LevelId { get; set; }
        public EnumScoringFormulaType ScoringFormulaType { get; set; }
        public CategoryModel? Program { get; set; }
        public LevelModel? Level { get; set; }
        public Guid OriginalId { get; set; }
        public Guid? ProjectId { get; set; }
        public bool IsArchive { get; set; }
        public int Version { get; set; }
        public EnumVersionStatus VersionStatus { get; set; }
        public EnumVersion VersionType { get; set; }
        public bool IsActive { get; set; }
        public IList<TestSectionModel> TestSections { get; set; } = new List<TestSectionModel>();
    }
}
