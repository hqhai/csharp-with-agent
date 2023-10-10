// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.QuestionTypeConfigs.Questions
{
    using System.Text.Json.Serialization;

    public class ShortAnswerQuestionWordBaseQuestion
    {
        [JsonRequired]
        public string? Name { get; set; }

        private IList<string>? _contents;

        [JsonRequired]
        public IList<string>? Contents
        {
            get { return _contents; }
            set
            {
                _contents = value;
                if (_contents != null && _contents.Any())
                {
                    Content = _contents.ToList();
                }
            }
        }

        public IList<string>? Content { get; set; }
        public bool IsSpeakRequired { get; set; }
    }
}
