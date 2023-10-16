// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Domain.Models.QueryModels.SupportQuestions
{
    using Fsel.Core.Base.BaseModels;

    public class SearchSupportQuestionQueryModel : BaseQueryModel
    {
        public Guid? SupportCategoryId { get; set; }
        public bool? IsFrequent { get; set; }
    }
}
