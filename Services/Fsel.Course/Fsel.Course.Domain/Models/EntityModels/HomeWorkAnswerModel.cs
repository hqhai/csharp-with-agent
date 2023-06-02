// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Domain.Entities;

    public class HomeWorkAnswerModel : BaseModel
    {
        public object? Answer { get; set; }

        public int CorrectCount { get; set; }

        public HomeWorkQuestion? HomeWorkQuestion { get; set; }

        public HomeWorkResult? HomeWorkResult { get; set; }

        public Guid? HomeWorkQuestionId { get; set; }

        public Guid? HomeWorkResultId { get; set; }
    }
}
