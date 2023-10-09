// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities.QuestionTypeConfigs.Questions
{
    using System.Text.Json.Serialization;

    public class ShortAnswerQuestionWordBaseQuestion
    {
        [JsonRequired]
        public string? Name { get; set; }

        [JsonRequired]
        public IList<string>? Contents { get; set; }

        public IList<string>? Content
        {
            get
            {
                if (Contents != null && Contents.Any())
                {
                    Content = Contents.ToList();
                }
                return Content;
            }
            set { Contents = value; }
        }

        public bool IsSpeakRequired { get; set; }
    }
}
