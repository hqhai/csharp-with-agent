// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Models.QueryModels
{
    using Fsel.Core.Base.BaseModels;

    public class SearchTeacherFreeTimeByCsoQueryModel : BaseQueryModel
    {
        public DateTime? EndDate { get; set; }

        public DateTime? StartDate { get; set; }

        public DayOfWeek? DayOfWeek { get; set; }

        public bool? Priority { get; set; }

        public Guid? LiveTimeFrameId { get; set; }
    }
}
