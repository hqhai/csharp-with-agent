// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Domain.Models.EntityModels
{
    using System;
    using Fsel.Cms.PlanetDefender.Domain.Enums;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class StudentGameInfoModel : BaseModel
    {
        public EnumGameCourseLevel? Level { get; set; }
        public Guid StudentId { get; set; }
        public EnumGender Gender { get; set; }
    }
}
