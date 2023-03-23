// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Entities.SurveyQuestionConfigs.SurverQuestionSources
{
    public static class ChooseDirectionQuestionSourse
    {
        public static ChooseDirectionQuestion ChooseDirectionQuestion
        {
            get
            {
                return new ChooseDirectionQuestion
                {
                    ChooseDirectionQuestions = new List<ChooseDirectionQuestionAnswers>
                    {
                        new ChooseDirectionQuestionAnswers
                        {
                            Id = 1,
                            Content = "Bạn đã biết một chút Tiếng Anh"
                        },
                        new ChooseDirectionQuestionAnswers
                        {
                            Id = 2,
                            Content = "Đây là lần đầu bạn học Tiếng Anh"
                        }
                    }
                };
            }
        }
    }
}
