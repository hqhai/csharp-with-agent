using Fsel.Cms.PlanetDefender.Domain.Enums;
using Fsel.Core.Entities;
using Fsel.Shared.Enums;

namespace Fsel.Cms.PlanetDefender.Domain.Entities
{
    public class StudentGameInfo : Entity
    {
        public EnumGameCourseLevel Level { get; set; }
        public Guid StudentId { get; set; }
        public string? NickName { get; set; }
        public EnumGender Gender { get; set; }
    }
}
