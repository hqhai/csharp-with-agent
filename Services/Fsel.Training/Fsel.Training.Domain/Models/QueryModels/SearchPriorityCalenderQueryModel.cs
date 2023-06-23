// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Models.QueryModels
{
    using Fsel.Core.Base.BaseModels;
    using Fsel.Shared.Enums;

    public class SearchPriorityCalenderQueryModel : BaseQueryModel
    {
        public DateTime? EndTime { get; set; }

        public DateTime? StartTime { get; set; }
        public EnumDayOfWeek? DayOfWeek { get; set; }

        public int? Priority { get; set; }
    }
}
