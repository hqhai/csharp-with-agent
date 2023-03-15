using Fsel.Common.Enums;
using Fsel.Core.Base.BaseModels;

namespace Fsel.Course.Domain.Models.CommandModels.PlacementTests
{
    public class UpdatePlacementTestCommandModel : BaseCommandModel
    {
        public string? Name { get; set; }

        public string? InstructionContent { get; set; }

        public bool IsActive { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }
    }
}
