// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using Fsel.Core.Entities;

    public class HomeWorkRetry : Entity
    {
        public HomeWork? HomeWork { get; set; }
        public Guid HomeWorkId { get; set; }
        public Guid StudentId { get; set; }
        public int NumberRetry { get; set; }
        public ICollection<HomeWorkExtraPracticeResult> HomeWorkExtraPracticeResults { get; set; } = new List<HomeWorkExtraPracticeResult>();
    }
}
