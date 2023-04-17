// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Doman.Entities
{
    using Fsel.Core.Entities;

    public class ClassStudent : Entity
    {
        public Class? Class { get; set; }
        public Guid ClassId { get; set; }
        public Guid StudentId { get; set; }
    }
}
