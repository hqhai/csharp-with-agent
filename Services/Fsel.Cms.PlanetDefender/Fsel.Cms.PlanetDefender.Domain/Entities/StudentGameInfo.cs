using Fsel.Core.Entities;
using Fsel.Shared.Enums;

namespace Fsel.Cms.PlanetDefender.Domain.Entities
{
    public class StudentGameInfo : Entity
    {
        public EnumGameCourseLevel Level { get; set; }
        public Guid StudentId { get; set; }
    }
}
