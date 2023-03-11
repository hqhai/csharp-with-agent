using Fsel.Core.Entities;

namespace Fsel.Course.Domain.Entities
{
    public class UnitMockFinalTest : Entity
    {
        public Unit? Unit { get; set; }

        public MockFinalTest? MockFinalTest { get; set; }

        public Guid UnitId { get; set; }

        public Guid MockFinalTestId { get; set; }
    }
}