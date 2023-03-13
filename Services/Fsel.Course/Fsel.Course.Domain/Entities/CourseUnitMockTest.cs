using Fsel.Core.Entities;

namespace Fsel.Course.Domain.Entities
{
    public class CourseUnitMockTest : Entity
    {
        public int OrderNumber { get; set; }

        public Unit? Unit { get; set; }

        public Course? Course { get; set; }

        public MockTest? MockTest { get; set; }

        public Guid UnitId { get; set; }

        public Guid CourseId { get; set; }

        public Guid MockTestId { get; set; }
    }
}