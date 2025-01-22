// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Interaction.Domain.Entities;
    using Fsel.Shared.Enums;

    public class CustomerSurveyGroupModel : BaseModel
    {
        public EnumSurveyGroupStatus Status { get; set; }

        public Guid UserId { get; set; }

        public EnumSurveyFormType SurveyFormType { get; set; }

        public Guid? CompetitionEventId { get; set; }

        public double? Coin { get; set; }

        public IList<CustomerSurvey>? CustomerSurveys { get; set; }
    }
}
