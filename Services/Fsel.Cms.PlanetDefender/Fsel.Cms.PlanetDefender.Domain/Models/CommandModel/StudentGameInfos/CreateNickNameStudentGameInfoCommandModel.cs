// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Domain.Models.CommandModel.StudentGameInfos
{
    using System;

    public class CreateNickNameStudentGameInfoCommandModel
    {
        public string? NickName { get; set; }

        public Guid? StudentId { get; set; }
    }
}
