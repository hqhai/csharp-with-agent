// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Entities.SurveyQuestionConfigs.SurverQuestionSources
{
    using Fsel.Interaction.Domain.Enums;

    public static class AgeGenderQuestionSourse
    {
        public static AgeGenderQuestion AgeGenderQuestion
        {
            get
            {
                return new AgeGenderQuestion
                {
                    AgeGenderQuestions = new List<AgeGenderQuestionAnswers>
                    {
                        new AgeGenderQuestionAnswers
                        {
                            Id = 1,
                             Gender = EnumGender.Male
                        },
                        new AgeGenderQuestionAnswers
                        {
                            Id = 2,
                             Gender = EnumGender.Female
                        },
                        new AgeGenderQuestionAnswers
                        {
                            Id = 3,
                            Gender = EnumGender.Other
                        },
                    }
                };
            }
        }
    }
}
