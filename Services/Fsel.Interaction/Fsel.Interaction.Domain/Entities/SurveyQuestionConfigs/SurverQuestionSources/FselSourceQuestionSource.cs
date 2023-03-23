// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Entities.SurveyQuestionConfigs.SurverQuestionSources
{
    public static class FselSourceQuestionSource
    {
        public static FselSourceQuestion FselSourceQuestion
        {
            get
            {
                return new FselSourceQuestion
                {
                    FselSourceQuestions = new List<FselSourceQuestionAnswers>
                    {
                        new FselSourceQuestionAnswers
                        {
                            Id = 1,
                            Content = "Tìm kiếm Google",
                            Image = "Google"
                        },
                        new FselSourceQuestionAnswers
                        {
                            Id = 2,
                            Content = "Facebook",
                            Image = "Facebook"
                        },
                        new FselSourceQuestionAnswers
                        {
                            Id = 3,
                            Content = "Youtube",
                            Image = "Youtube"
                        },
                        new FselSourceQuestionAnswers
                        {
                            Id = 4,
                            Content = "Tiktok",
                            Image = "Tiktok"
                        },
                        new FselSourceQuestionAnswers
                        {
                            Id = 5,
                            Content = "Bạn bè/Gia đình",
                            Image = "Bạn bè/Gia đình"
                        },
                        new FselSourceQuestionAnswers
                        {
                            Id = 6,
                            Content = "Tin tức/Báo chí/Blog",
                            Image = "Tin tức/Báo chí/Blog"
                        },
                        new FselSourceQuestionAnswers
                        {
                            Id = 7,
                            Content = "Tivi",
                            Image = "Tivi"
                        },
                        new FselSourceQuestionAnswers
                        {
                            Id = 8,
                            Content = "Khác....",
                            Image = "Khác...."
                        }
                    }
                };
            }
        }
    }
}
