// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Parents
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Fsel.Identity.Domain.Entities;

    public class UpdateParentCommandModel
    {
        public string? Gender { get; set; }
        public string? Occupation { get; set; }
        public Guid HumanId { get; set; }
    }
}
