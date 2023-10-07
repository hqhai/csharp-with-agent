// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Domain.Models.QueryModels.StudentGameInfos
{
    using System;
    using Fsel.Cms.PlanetDefender.Domain.Enums;
    using Fsel.Core.Base.BaseModels;

    public class ChooseCharacterGenderCommandModel : BaseCommandModel
    {
        public EnumGender Gender { get; set; }
    }
}
