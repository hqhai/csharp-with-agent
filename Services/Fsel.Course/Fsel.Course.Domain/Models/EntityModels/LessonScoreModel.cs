using Fsel.Core.Base.BaseModels;

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class LessonScoreModel : BaseModel
    {
        public IList<LessonSkillScoreModel>? LessonSkillScores { get; set; }
        public double Percent { get; set; }
    }
}
