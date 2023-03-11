using Fsel.Core.Base.BaseModels;
using Fsel.Course.Domain.Enums;

namespace Fsel.Course.Common.Models.Entities
{
    public class QuestionModel : BaseEntityModel
    {
        public EnumQuestionType QuestionType { get; set; }
        public bool IsSave { get; set; }

        public string? Config { get; set; }
    }
}