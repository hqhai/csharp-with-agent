// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Fsel.Course.Domain.IEntities;

    public class MockTestResultReportModel : TestResultReportModel, ITokenResult
    {
        public bool IsTeacherGraded { get; set; }
    }
}
