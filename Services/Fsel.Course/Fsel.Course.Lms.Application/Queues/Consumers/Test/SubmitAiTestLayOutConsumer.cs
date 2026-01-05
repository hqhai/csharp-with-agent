// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queues.Consumers.Test
{
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Course.Lms.Application.Services.TestServices.Interface;

    public class SubmitAiTestLayOutConsumer : BaseConsumer<BaseQueueModel>
    {
        private readonly ITestAiLayoutService _testAiLayoutService;

        public SubmitAiTestLayOutConsumer(AuthContext authContext,
            ITestAiLayoutService testAiLayoutService,
            Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _testAiLayoutService = testAiLayoutService;
        }

        public override async Task ConsumeQueue(BaseQueueModel? message)
        {
            if (message == null || !Guid.TryParse(message.QueueId, out Guid testSectionResultId))
            {
                return;
            }
            await _testAiLayoutService.EvaluateAsync(testSectionResultId, CancellationToken.None);
        }
    }
}
