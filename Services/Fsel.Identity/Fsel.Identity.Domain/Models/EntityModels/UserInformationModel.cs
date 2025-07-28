// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    using System;
    using Fsel.Shared.Enums;

    public class UserInformationModel
    {
        public Guid? Id { get; set; }
        public string? UserName { get; set; }
        public bool EmailConfirmed { get; set; }
        public EnumUserStatus? Status { get; set; }
    }
}
