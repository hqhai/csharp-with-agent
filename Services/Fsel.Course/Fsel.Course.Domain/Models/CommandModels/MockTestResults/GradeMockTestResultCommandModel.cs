// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.CommandModels.MockTestResults
{
    public class GradeMockTestResultCommandModel
    {
        public Guid MockTestResultId { get; set; }
        public IList<CreateMockTestScoreCommandModel>? MockTestScores { get; set; }
    }
}
