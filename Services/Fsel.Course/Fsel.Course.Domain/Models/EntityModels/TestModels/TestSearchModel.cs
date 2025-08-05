// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.TestModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class TestSearchModel : BaseModel
    {
        public string? Name { get; set; }
        public string? Code { get; set; }
        public Guid OriginalId { get; set; }
        public Guid? LevelId { get; set; }
        public string? LevelName { get; set; }
        public Guid? ProgramId { get; set; }
        public string? ProgramName { get; set; }
        public string? Overview { get; set; }
        public List<string>? Skills { get; set; }
        public IList<EnumTestLayoutType?>? LayoutTypes { get; set; }
        public bool IsActive { get; set; }
    }
}
