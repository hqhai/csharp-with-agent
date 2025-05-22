// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    using System;

    public class UserInformationModel
    {
        public Guid? Id { get; set; }
        public string? UserName { get; set; }
        public bool EmailConfirmed { get; set; }
    }
}
