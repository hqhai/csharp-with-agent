// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Course.Domain.IEntities;

    public class TestResultReportModel : BaseLearnResultModel, ITokenResult
    {
        public double? Score { get; set; }
        public int? TokenFirstTime { get; set; }
        public int? TokenLastTime { get; set; }
    }
}
