// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Entities.SurveyQuestionConfigs.SurverQuestionSources
{
    public static class ChooseLanguageQuestionSourse
    {
        public static ChooseLanguageQuestion ChooseLanguageQuestion
        {
            get
            {
                return new ChooseLanguageQuestion
                {
                    ChooseLanguageQuestions = new List<ChooseLanguageQuestionAnswers>
                    {
                        new ChooseLanguageQuestionAnswers
                        {
                            Id = 1,
                            Content = "Văn hóa",
                            Image = "Văn hóa"
                        },
                       new ChooseLanguageQuestionAnswers
                        {
                            Id = 2,
                            Content = "Du lịch",
                            Image = "Du lịch"
                        },
                        new ChooseLanguageQuestionAnswers
                        {
                            Id = 3,
                            Content = "Kết bạn và chia sẻ",
                            Image = "Kết bạn và chia sẻ"
                        },
                        new ChooseLanguageQuestionAnswers
                        {
                            Id = 4,
                            Content = "Học tập",
                            Image = "Học tập"
                        },
                        new ChooseLanguageQuestionAnswers
                        {
                            Id = 5,
                            Content = "Cơ hội nghề nghiệp",
                            Image =  "Cơ hội nghề nghiệp"
                        },
                        new ChooseLanguageQuestionAnswers
                        {
                            Id = 6,
                            Content = "Khác....",
                            Image = "Khác...."
                        },
                    }
                };
            }
        }
    }
}
