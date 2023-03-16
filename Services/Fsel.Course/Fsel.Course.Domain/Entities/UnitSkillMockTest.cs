using System.ComponentModel.DataAnnotations;
using Fsel.Core.Entities;
using Fsel.Course.Domain.Enums.ErrorCodes;

namespace Fsel.Course.Domain.Entities
{
    public class UnitSkillMockTest : Entity
    {
        public Unit? Unit { get; set; }

        public MockTest? MockTest { get; set; }

        [Required(ErrorMessage = nameof(EnumUnitErrorCode.U01V))]
        public Guid UnitId { get; set; }

        [Required(ErrorMessage = nameof(EnumMockTestErrorCode.MT01V))]
        public Guid MockTestId { get; set; }
    }
}
