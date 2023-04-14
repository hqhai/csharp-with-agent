// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Core.Base.BaseModels;

    public class HomeWorkAnswerModel : BaseModel
    {
        public string? AnswerStr { get; set; }

        public int CorrectCount { get; set; }

        public HomeWorkQuestion? HomeWorkQuestion { get; set; }

        public HomeWorkResult? HomeWorkResult { get; set; }

        public Guid? HomeWorkQuestionId { get; set; }

        public Guid? HomeWorkResultId { get; set; }
    }
}
