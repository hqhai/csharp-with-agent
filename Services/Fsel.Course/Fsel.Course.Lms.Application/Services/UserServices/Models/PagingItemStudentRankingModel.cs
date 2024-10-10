// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.UserServices.Models
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Models.ShareModels;

    public class PagingItemStudentRankingModel : PagingItemsModel<StudentRankingModel>
    {
        public WeekEvent? WeekEvent { get; set; }
    }
}
