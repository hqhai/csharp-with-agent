// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Course.Domain.IEntities;

    public class TestResultReportModel : BaseLearnResultModel, ICorrectQuestion
    {
        public double? Score { get; set; }
        public int CorrectQuestion { get; set; }
        public int TotalQuestion { get; set; }
    }
}
