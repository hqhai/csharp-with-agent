// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Services.CMSPlanetDefenderService.Models
{
    using System;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class StudentGameInfoModel : BaseModel
    {
        public EnumGameCourseLevel? Level { get; set; }
        public Guid StudentId { get; set; }
    }
}
