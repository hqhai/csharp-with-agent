// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.IEntities
{
    public interface IModuleLifeCycle
    {
        public DateTime? ProcessDate { get; set; }
        public DateTime? CompletionDate { get; set; }
    }
}
