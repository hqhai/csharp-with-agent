using Fsel.Core.Base.BaseModels;
using Fsel.Course.Domain.Enums;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fsel.Course.Common.Models.Commands.Question
{
    public class UpdateQuestionCommandModel
    {
        public Guid Id { get; set; }
        public EnumQuestionType QuestionType { get; set; }
        public bool IsSave { get; set; }
        public string? Config { get; set; }
    }
}