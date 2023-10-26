using Fsel.Cms.PlanetDefender.Domain.Enums;
using Fsel.Common.Attributes;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Core.Entities;
using Fsel.Shared.Enums;

namespace Fsel.Cms.PlanetDefender.Domain.Entities
{
    public class StudentGameInfo : Entity
    {
        public EnumGameCourseLevel Level { get; set; }
        public Guid StudentId { get; set; }

        [RegexValid(Regex = "^[A-Za-z0-9]{4,20}$s", ErrorMessage = nameof(EnumSystemErrorCode.InValidFormat))]
        public string? NickName { get; set; }
        public EnumGender Gender { get; set; }
    }
}
