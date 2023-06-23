// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Models.QueryModels
{
    using Fsel.Core.Base.BaseModels;

    public class SearchTeacherFreeTimeByCsoQueryModel : BaseQueryModel
    {
        public DateTime? EndTime { get; set; }

        public DateTime? StartTime { get; set; }
        public DayOfWeek? DayOfWeek { get; set; }

        public bool? Priority { get; set; }

        public DateTime? TimeFrameEndTime { get; set; }
        public DateTime? TimeFrameStartTime { get; set; }
    }
}
