// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using Fsel.Core.Entities;

    public class HomeWorkRetry : Entity
    {
        public int NumberRetry { get; set; }
        public CurriculumConfig? Curriculum { get; set; }
        public Guid? CurriculumId { get; set; }
        public HomeWork? HomeWork { get; set; }
        public Guid HomeWorkId { get; set; }
        public Guid StudentId { get; set; }
        public ICollection<HomeWorkExtraPracticeResult> HomeWorkExtraPracticeResults { get; set; } = new List<HomeWorkExtraPracticeResult>();
    }
}
