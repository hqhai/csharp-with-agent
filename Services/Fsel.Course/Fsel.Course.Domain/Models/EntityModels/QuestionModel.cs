using Fsel.Core.Base.BaseModels;
using Fsel.Course.Domain.Enums;

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class QuestionModel : BaseEntityModel
    {
        public EnumQuestionType QuestionType { get; set; }
        public bool IsSave { get; set; }

        public object? Config { get; set; }
    }
}
