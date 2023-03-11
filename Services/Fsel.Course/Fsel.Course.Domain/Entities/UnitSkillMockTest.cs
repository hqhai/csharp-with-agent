using Fsel.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace Fsel.Course.Domain.Entities
{
    public class UnitSkillMockTest : Entity
    {
        public Unit? Unit { get; set; }

        public MockTest? MockTest { get; set; }

        public Guid UnitId { get; set; }

        public Guid MockTestId { get; set; }
    }
}