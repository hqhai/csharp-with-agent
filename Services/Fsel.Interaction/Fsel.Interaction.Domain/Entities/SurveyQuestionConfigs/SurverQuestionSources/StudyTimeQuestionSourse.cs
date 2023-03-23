// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Entities.SurveyQuestionConfigs.SurverQuestionSources
{
    public static class StudyTimeQuestionSourse
    {
        public static StudyTimeQuestion StudyTimeQuestion
        {
            get
            {
                return new StudyTimeQuestion
                {
                    StudyTimeQuestions = new List<StudyTimeQuestionAnswers>
                    {
                        new StudyTimeQuestionAnswers
                        {
                            Id = 1,
                            Content = "8 - 10 am"
                        },
                        new StudyTimeQuestionAnswers
                        {
                            Id = 2,
                            Content = "1 - 3 pm"
                        },
                        new StudyTimeQuestionAnswers
                        {
                            Id = 3,
                            Content = "3 - 5 pm"
                        },
                        new StudyTimeQuestionAnswers
                        {
                            Id = 4,
                            Content = "5 - 9 pm"
                        },
                        new StudyTimeQuestionAnswers
                        {
                            Id = 5,
                            Content = "7 - 10 pm"
                        }
                    }
                };
            }
        }
    }
}
