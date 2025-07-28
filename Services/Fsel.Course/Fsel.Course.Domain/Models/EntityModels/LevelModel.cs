// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;

    public class LevelModel : BaseModel
    {
        public string? Name { get; set; }

        public string? Code { get; set; }

        public string? Description { get; set; }

        public int LevelOrder { get; set; }

        public IList<SkillViewModel>? Skils { get; set; }
    }
}
