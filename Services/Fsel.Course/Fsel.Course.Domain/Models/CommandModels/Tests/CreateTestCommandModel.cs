// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.Tests
{
    using System.Collections.Generic;
    using Fsel.Course.Domain.Models.CommandModels.TestSections;

    public class CreateTestCommandModel
    {
        public string? Name { get; set; }
        public string? Code { get; set; }
        public Guid? ProgramId { get; set; }
        public Guid? LevelId { get; set; }
        public IList<CreateTestSectionCommandModel> TestSections { get; set; } = new List<CreateTestSectionCommandModel>();
    }
}
