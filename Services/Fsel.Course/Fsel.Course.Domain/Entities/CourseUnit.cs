using Fsel.Core.Entities;

namespace Fsel.Course.Domain.Entities
{
    public class CourseUnit : Entity
    {
        public Unit? Unit { get; set; }

        public Course? Course { get; set; }

        public Guid UnitId { get; set; }

        public Guid CourseId { get; set; }
    }
}