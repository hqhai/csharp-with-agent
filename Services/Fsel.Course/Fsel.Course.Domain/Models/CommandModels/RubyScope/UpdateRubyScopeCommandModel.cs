// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.RubyScope
{
    using Fsel.Core.Base.BaseModels;

    public class UpdateRubyScopeCommandModel : BaseCommandModel
    {
        public string? BaseTextHash { get; set; }
    }
}
