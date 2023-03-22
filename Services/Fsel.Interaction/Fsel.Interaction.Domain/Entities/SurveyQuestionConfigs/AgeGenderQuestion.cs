// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Entities.SurveyQuestionConfigs
{
    public class AgeGenderQuestion
    {
        public DateTime? Birthday { get; set; }

        public IList<GenderQuestion>? GenderQuestions { get; set; }
    }

    public class GenderQuestion
    {
        public string? Gender { get; set; }
        public int Id { get; set; }
    }

    public class AgeGenderAnswer
    {
        public DateTime? Birthday { get; set; }

        public GenderQuestion? GenderQuestion { get; set; }
    }
}
