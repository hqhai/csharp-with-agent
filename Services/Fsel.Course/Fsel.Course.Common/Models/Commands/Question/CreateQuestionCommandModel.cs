using Fsel.Course.Domain.Entities;
using Fsel.Course.Domain.Enums.ErrorCodes;
using Fsel.Course.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Course.Common.Models.Commands.Question
{
    public class CreateQuestionCommandModel
    {
        public EnumQuestionType QuestionType { get; set; }
        public bool IsSave { get; set; }
        public string? Config { get; set; }
    }
}