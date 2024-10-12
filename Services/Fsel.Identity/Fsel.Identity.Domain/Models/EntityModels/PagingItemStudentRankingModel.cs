// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.Models.EntityModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Models.ShareModels;

    public class PagingItemStudentRankingModel : PagingItemsModel<StudentRankingModel>
    {
        public WeekEvent? WeekEvent { get; set; }
    }
}
