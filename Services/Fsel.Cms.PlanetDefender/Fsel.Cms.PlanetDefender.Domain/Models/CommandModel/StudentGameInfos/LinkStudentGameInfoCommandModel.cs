// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Domain.Models.CommandModel.StudentGameInfos
{
    using System;

    public class LinkStudentGameInfoCommandModel
    {
        public Guid UserId { get; set; }

        public bool IsChooseUser { get; set; }
    }
}
