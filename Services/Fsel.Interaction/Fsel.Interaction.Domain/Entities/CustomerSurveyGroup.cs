// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Entities
{
    using Fsel.Core.Entities;
    using Fsel.Shared.Enums;

    public class CustomerSurveyGroup : Entity
    {
        public EnumSurveyGroupStatus Status { get; set; }

        public Guid UserId { get; set; }

        public EnumSurveyFormType SurveyFormType { get; set; }

        public Guid? CompetitionEventId { get; set; }

        public double? Coin { get; set; }

        public IList<CustomerSurvey> CustomerSurveys { get; set; } = new List<CustomerSurvey>();
    }
}
