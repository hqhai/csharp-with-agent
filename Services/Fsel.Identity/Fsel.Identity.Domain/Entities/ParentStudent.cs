// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Entities
{
    using System;
    using Fsel.Core.Entities;

    public class ParentStudent : Entity
    {
        public Parent? Parent { get; set; }
        public Guid? ParentId { get; set; }
        public Student? Student { get; set; }
        public Guid? StudentId { get; set; }
    }
}
