// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.Tests
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Models.CommandModels.TestSections;

    public class UpdateTestCommandModel : BaseCommandModel
    {
        public string? Name { get; set; }
        public string? Code { get; set; }
        public Guid LevelId { get; set; }
        public Guid ProgramId { get; set; }
        public IList<UpdateTestSectionCommandModel> TestSections { get; set; } = new List<UpdateTestSectionCommandModel>();
    }
}
