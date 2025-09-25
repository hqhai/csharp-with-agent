// Copyright (c) Atlantic. All rights reserved.

using Fsel.Course.Domain.Models.QueryModels.Ruby;

namespace Fsel.Course.Domain.Models.CommandModels.RubyScope
{
    using Fsel.Core.Base.BaseModels;

    public class CreateRubyByScopeCommandModel : BaseModel
    {
        public Guid RubyId { get; set; }
        public Guid ScopeId { get; set; }
        public string? Html { get; set; }
        public List<RubyAnnotaionModel>? Annotations { get; set; }
    }
}
