// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Models.EntityModels.Bases
{
    using Fsel.Core.Base.BaseModels;

    public class BaseAnswerModel : BaseModel
    {
        public object? Answer { get; set; }
        public int CorrectCount { get; set; }
        public bool? IsCorrect { get; set; }
    }
}
