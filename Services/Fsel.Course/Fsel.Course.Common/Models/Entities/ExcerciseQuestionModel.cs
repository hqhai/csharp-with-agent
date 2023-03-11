using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Course.Common.Models.Entities
{
    public class ExcerciseQuestionModel
    {
        public Guid ExcerciseId { get; set; }

        public Guid QuestionId { get; set; }

        public List<QuestionModel>? Questions { get; set; }
    }
}