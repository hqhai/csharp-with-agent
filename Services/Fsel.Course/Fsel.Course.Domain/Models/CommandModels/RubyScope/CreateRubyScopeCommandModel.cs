// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.RubyScope
{
    using Fsel.Core.Base.BaseModels;

    public class CreateRubyScopeCommandModel : BaseCommandModel
    {
        public string HostType { get; set; } = default!;
        public Guid HostId { get; set; }
        public string FieldKey { get; set; } = default!;
        public string? BaseTextHash { get; set; }
    }
}
