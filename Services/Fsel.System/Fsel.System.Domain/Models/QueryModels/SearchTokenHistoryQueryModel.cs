// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.QueryModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class SearchTokenHistoryQueryModel : BaseQueryModel
    {
        public Guid? UserId { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }
        public Guid? CourseResultId { get; set; }
        public EnumTokenHistoryType? Type { get; set; }
    }
}
