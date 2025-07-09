// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.TestModels
{
    using Fsel.Core.Base.BaseModels;

    public class TestModel : BaseModel
    {
        public string? Name { get; set; }
        public string? Code { get; set; }
        public CategoryModel? Program { get; set; }
        public LevelModel? Level { get; set; }
        public IList<TestSectionModel> TestSections { get; set; } = new List<TestSectionModel>();
    }
}
