// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Models.EntityModels.Bases
{
    public class BaseAnswerModel
    {
        public Guid Id { get; set; }
        public object? Answer { get; set; }
        public int CorrectCount { get; set; }
        public bool? IsCorrect { get; set; }
    }
}
