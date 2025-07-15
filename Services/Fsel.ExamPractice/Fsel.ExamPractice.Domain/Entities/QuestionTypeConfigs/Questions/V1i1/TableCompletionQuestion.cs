// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Entities.QuestionTypeConfigs.Questions.V1i1
{
    public class TableCompletionQuestion : ConfigQuestionV1
    {
        public IList<ConfigAnswerV1> Columns { get; set; } = new List<ConfigAnswerV1>();
        public IList<RowQuestion> Rows { get; set; } = new List<RowQuestion>();
    }

    public class RowQuestion : ConfigAnswerV1
    {
        public string? ColumnKey { get; set; }
    }
}
