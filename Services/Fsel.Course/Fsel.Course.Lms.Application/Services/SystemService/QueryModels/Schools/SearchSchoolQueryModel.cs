// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.SystemService.QueryModels
{
    using Fsel.Core.Base.BaseModels;

    public class SearchSchoolQueryModel : BaseQueryModel
    {
        public Guid? LocationId { get; set; }
        public string? ListLocationId { get; set; }
    }
}
