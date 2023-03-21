// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    using System;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Identity.Domain.Entities;

    public class ParentModel : BaseModel
    {
        public string? Gender { get; set; }
        public string? Occupation { get; set; }

        public Human? Human { get; set; }
        public Guid HumanId { get; set; }
    }
}
