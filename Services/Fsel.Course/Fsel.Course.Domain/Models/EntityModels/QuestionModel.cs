// Copyright (c) Atlantic. All rights reserved.

using Fsel.Shared.Enums;

namespace Fsel.Course.Domain.Models.EntityModels
{
    public class QuestionModel
    {
        public Guid Id { get; set; }
        public EnumQuestionType QuestionType { get; set; }
        public int CorrectTotal { get; set; }
        public bool Ungraded { get; set; }

        public string? Explanation { get; set; }
        public object? Config { get; set; }
        public string? Description { get; set; }
        public IList<int>? SubQuestionIndexs { get; set; }
        public int SubQuestionNumber { get; set; }
        public EnumCorrectStatus? CorrectStatus { get; set; }
        public AnswerModel? ResultAnswer { get; set; }
        public bool? IsReportExplanation { get; set; }
        public IList<ExplanationTranslationModel>? Explanations { get; set; }
        public Guid? SectionId { get; set; }
    }
}
