// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    using System;
    using Fsel.Core.Base.BaseModels;

    public class ParentModel : BaseModel
    {
        public string? Occupation { get; set; }
        public Guid HumanId { get; set; }
        public HumanProfileModel? Human { get; set; }
    }
}
