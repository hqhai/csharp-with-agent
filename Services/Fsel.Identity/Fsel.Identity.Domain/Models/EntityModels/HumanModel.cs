// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    using System;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Identity.Domain.Entities;

    public class HumanModel : BaseEntityModel
    {
        public string? FullName { get; set; }
    }
}
