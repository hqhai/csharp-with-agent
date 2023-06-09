// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using Fsel.Core.Entities;

    public class SectionQuestion : Entity
    {
        public Question? Question { get; set; }
        public Section? Section { get; set; }
        public SectionPart? SectionPart { get; set; }
        public Guid? QuestionId { get; set; }
        public Guid? SectionId { get; set; }
        public Guid? SectionPartId { get; set; }
        public ICollection<PlacementTestAnswer> PlacementTestAnswers { get; set; } = new List<PlacementTestAnswer>();
        public ICollection<FinalTestAnswer> FinalTestAnswers { get; set; } = new List<FinalTestAnswer>();
        public ICollection<MockTestAnswer> MockTestAnswers { get; set; } = new List<MockTestAnswer>();
    }
}
