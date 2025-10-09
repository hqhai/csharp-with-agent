// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.Aggregates
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Shared.Enums;

    public class TestAnswerLeaf : ResultComponent
    {
        public TestAnswer TestAnswer => (TestAnswer)Result;

        public override bool IsBelongTo(Guid id)
        {
            return Result.Id == id;
        }

        public override async Task Submit()
        {
            TestAnswer.Status = EnumAnswerStatus.Done;
            await Task.CompletedTask;
        }
    }
}
