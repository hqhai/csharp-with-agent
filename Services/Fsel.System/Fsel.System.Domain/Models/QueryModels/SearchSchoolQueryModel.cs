// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.QueryModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class SearchSchoolQueryModel : BaseQueryModel
    {
        public Guid? LocationId { get; set; }

        public EnumEducationLevel? EducationLevel { get; set; }
    }
}
