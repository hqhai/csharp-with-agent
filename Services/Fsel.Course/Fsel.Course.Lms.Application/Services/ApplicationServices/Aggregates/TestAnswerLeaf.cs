// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.Aggregates
{
    using System;
    using System.Threading.Tasks;
    using Domain.Models.EntityModels;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Models.EntityModels.PlacementTestModels;
    using Fsel.Shared.Enums;

    public class TestAnswerLeaf : ResultComponent
    {
        public TestAnswer TestAnswer => Result as TestAnswer;

        public override BaseTestStateModel ExportState()
        {
            return new QuestionStateModel
            {
                QuestionId = TestAnswer.QuestionId,
                TestAnswerId = TestAnswer.Id,
                Answer = new AnswerModel { Answer = TestAnswer.Answer, CorrectCount = TestAnswer.CorrectCount, IsCorrect = TestAnswer.IsCorrect, },
                Status = TestAnswer.Status == EnumAnswerStatus.Done ? EnumResultStatus.Done : EnumResultStatus.Process,
                UpdatedDate = TestAnswer?.UpdatedDate ?? TestAnswer?.CreatedDate,
            };
        }

        public override BaseTestStateModel ExportForTestState()
        {
            return new QuestionStateModel
            {
                QuestionId = TestAnswer.QuestionId,
                TestAnswerId = TestAnswer.Id,
                Answer = new AnswerModel { Answer = TestAnswer.Answer, CorrectCount = TestAnswer.CorrectCount, IsCorrect = TestAnswer.IsCorrect },
                Status = TestAnswer.Status == EnumAnswerStatus.Done ? EnumResultStatus.Done : EnumResultStatus.Process,
                UpdatedDate = TestAnswer?.UpdatedDate ?? TestAnswer?.CreatedDate
            };
        }

        public override bool IsBelongTo(Guid id)
        {
            return Result != null && Result.Id == id;
        }

        public override async Task Submit(SubmitContext context)
        {
            TestAnswer.Status = EnumAnswerStatus.Done;
            await Task.CompletedTask;
        }

        public override async Task SubmitTest(SubmitContext context)
        {
            TestAnswer.Status = EnumAnswerStatus.Done;
            await Task.CompletedTask;
        }
    }
}
