using Fsel.Cms.PlanetDefender.Domain.Enums;
using Fsel.Common.Attributes;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Core.Entities;
using Fsel.Shared.Enums;

namespace Fsel.Cms.PlanetDefender.Domain.Entities
{
    public class StudentGameInfo : Entity
    {
        public EnumGameCourseLevel CourseLevel { get; set; }
        public Guid StudentId { get; set; }

        [RegexValid(Regex = "^[A-Za-z0-9]{4,20}$s", ErrorMessage = nameof(EnumSystemErrorCode.InValidFormat))]
        public string? NickName { get; set; }

        public EnumGender Gender { get; set; }

        public int Level { get; set; }

        public Guid AvatarImageId { get; set; }

        public AvatarImage? AvatarImage { get; set; }
        public Guid TagNameId { get; set; }

        public StudentTagName? StudentTagName { get; set; }

        public ICollection<GameHistory> GameHistories { get; set; } = new List<GameHistory>();
        public ICollection<StudentSpaceShip> StudentSpaceShips { get; set; } = new List<StudentSpaceShip>();
    }
}
