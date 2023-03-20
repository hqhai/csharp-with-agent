using Fsel.Common.Enums;
using Fsel.Core.Base.BaseModels;

namespace Fsel.Course.Domain.Models.QueryModels.Units
{
    public class SearchUnitQueryModel : BaseQueyModel
    {
        public Guid? TeacherId { get; set; }

        public EnumCourseLevel CourseLevel { get; set; }
    }
}
