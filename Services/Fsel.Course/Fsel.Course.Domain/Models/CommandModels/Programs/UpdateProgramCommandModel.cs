// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.Programs
{
    using Fsel.Shared.Enums;

    public class UpdateProgramCommandModel
    {
        public Guid Id { get; set; }

        public string? Name { get; set; }

        public string? Code { get; set; }

        public EnumStatus Status { get; set; }

        public string? Description { get; set; }

        public IList<UpdateLevelCommandModel>? Levels { get; set; }
    }
}
