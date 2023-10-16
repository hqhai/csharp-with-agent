// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using Fsel.Core.Entities;

    public class ClassForumResultRandom : Entity
    {
        public Guid ClassForumId { get; set; }

        public Guid? ClassId { get; set; }

        public Guid ClassForumResultId { get; set; }
    }
}
