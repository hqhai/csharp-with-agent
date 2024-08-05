// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.QuestionTypeConfigs.Questions.V1i1
{
    public class TableCompletionQuestion
    {
        public IList<ConfigAnswerV1> Columns { get; set; } = new List<ConfigAnswerV1>();
        public IList<RowQuestion> Rows { get; set; } = new List<RowQuestion>();
        public IList<AnswerTable> AnswerTables { get; set; } = new List<AnswerTable>();
    }

    public class RowQuestion : ConfigAnswerV1
    {
        public string? ColumnKey { get; set; }
    }

    public class AnswerTable : ConfigAnswerV1
    {
        public Guid RowId { get; set; }
    }
}
