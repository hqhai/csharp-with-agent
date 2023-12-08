// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Course.Domain.IEntities;

    public class BaseLearnResultModel : BaseScoreResultModel, IHighestStreak, IWorkingTime
    {
        public int? HighestStreak { get; set; }
        public double WorkingTime { get; set; }
    }
}
