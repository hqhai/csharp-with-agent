// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Models.EntityModels.SkillModels;

    public class TestConfigSearchModel : BaseModel
    {
        public string? Name { get; set; }
        public string? Code { get; set; }
        public bool IsActive { get; set; }

        //Program
        public CategoryModel? Program { get; set; }
        public IList<SkillModel>? Skills { get; set; }
    }
}
