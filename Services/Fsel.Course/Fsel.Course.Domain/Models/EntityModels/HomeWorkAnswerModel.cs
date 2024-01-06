// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using System;

    public class HomeWorkAnswerModel : BaseAnswerModel
    {
        public Guid? HomeWorkQuestionId { get; set; }

        public Guid? HomeWorkResultId { get; set; }
    }
}
