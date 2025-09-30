// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.QueryModels.Ruby
{
    using Fsel.Course.Domain.Enums;

    public class RubyRenderModel
    {
        public EnumObjectType ObjectType { get; set; }
        public Guid ObjectId { get; set; }
    }
}
