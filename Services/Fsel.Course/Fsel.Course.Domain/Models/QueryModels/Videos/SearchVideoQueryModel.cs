using Fsel.Common.Enums;
using Fsel.Core.Base.BaseModels;
using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums;

namespace Fsel.Course.Domain.Models.QueryModels.Videos
{
    public class SearchVideoQueryModel : BaseQueyModel
    {
        public Guid? TeacherId { get; set; }
        public EnumCourseLevel? Level { get; set; }
        public EnumTimeCodeType? TimeCodeType { get; set; }
    }
}
