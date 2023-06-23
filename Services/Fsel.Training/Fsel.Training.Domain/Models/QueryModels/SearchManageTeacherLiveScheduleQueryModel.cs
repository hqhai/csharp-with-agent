// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Domain.Models.QueryModels
{
    using Fsel.Core.Base.BaseModels;

    public class SearchManageTeacherLiveScheduleQueryModel : BaseQueryModel
    {
        public DateTime? TimeFrameEndTime { get; set; }
        public DateTime? TimeFrameStartTime { get; set; }
    }
}
