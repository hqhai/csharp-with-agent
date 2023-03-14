// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.CommandModels.Humans
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Fsel.Identity.Domain.Models.CommandModels.Parents;
    using Fsel.Identity.Domain.Models.CommandModels.Students;

    public class CreateHumanCommandModel
    {
        public string? FullName { get; set; }

        public DateTime? Birthday { get; set; }

        public string? PhoneNumber { get; set; }

        public string? Address { get; set; }
        public string? Email { get; set; }

        public string? AvatarPath { get; set; }

        public Guid UserId { get; set; }
        public CreateParentCommandModel? Parent { get; set; }
        public CreateStudentCommandModel? Student { get; set; }
    }
}
