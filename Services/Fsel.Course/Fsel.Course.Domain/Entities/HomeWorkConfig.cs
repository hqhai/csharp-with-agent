// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using Fsel.Core.Entities;

    public class HomeWorkConfig : Entity
    {
        public int NumberRetry { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Guid HomeWorkId { get; set; }
        public Guid CurriculumId { get; set; }
        public HomeWork? HomeWork { get; set; }
        public CurriculumConfig? CurriculumConfig { get; set; }
        public ICollection<HomeWorkRetry> HomeWorkRetríes { get; set; } = new List<HomeWorkRetry>();
    }
}
