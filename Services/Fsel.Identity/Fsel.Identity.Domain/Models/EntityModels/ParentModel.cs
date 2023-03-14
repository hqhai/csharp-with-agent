// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.Enums.ErrorCodes;

    public class ParentModel : BaseEntityModel
    {
        public string? Gender { get; set; }
        public string? Occupation { get; set; }

        public Human? Human { get; set; }
        public Guid HumanId { get; set; }
    }
}
