// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Users
{
    using System;
    using Fsel.Identity.Domain.Enums;

    public class UpdateCodeStudentCommandModel
    {
        public EnumGender Gender { get; set; }
        public DateTime Birthday { get; set; }
    }
}
