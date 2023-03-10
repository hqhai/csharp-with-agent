using Fsel.Course.Domain.Enums.ErrorCodes;
using Fsel.Course.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fsel.Core.Base.BaseModels;

namespace Fsel.Course.Common.Models.Entities
{
    public class QuestionModel : BaseEntityModel
    {
        public EnumQuestionType QuestionType { get; set; }
        public bool IsSave { get; set; }

        public string? Config { get; set; }
    }
}