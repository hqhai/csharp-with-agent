// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.QuestionTypeConfigs.Questions.V1i1
{
    public class MatchingTaskQuestion
    {
        public string? Name { get; set; }

        private string? _content;

        public string? Content
        {
            get => !string.IsNullOrEmpty(_content) ? _content : Name;
            set => _content = value;
        }

        public IList<ConfigAnswerV1> Placeholders { get; set; } = new List<ConfigAnswerV1>();
        public IList<ConfigAnswerV1> Answers { get; set; } = new List<ConfigAnswerV1>();
    }
}
