// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Realtime.Application.Queues.Consumers
{
    using Fsel.Core.Base;
    using Fsel.Core.Extensions;
    using Fsel.Realtime.Application.Hubs;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Models.ShareModels;
    using Microsoft.AspNetCore.SignalR;

    public class ExamPracticeAIFeedBackConsumer : BaseConsumer<SubmitExamPracticeAiSpeakingResponseModel>
    {
        private readonly IHubContext<ExamPracticeWritingHub> _examPracticeHubContext;

        public ExamPracticeAIFeedBackConsumer(IHubContext<ExamPracticeWritingHub> examPracticeHubContext, AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _examPracticeHubContext = examPracticeHubContext;
        }

        public override async Task ConsumeQueue(SubmitExamPracticeAiSpeakingResponseModel? message)
        {
            if (message != null)
            {
                var examPracticeResultId = message.ExamPracticeResultId.ToString();
                await _examPracticeHubContext.GetGroup(examPracticeResultId!).SendAsync(RealtimeSettings.ExamPracticeWritingAIFeedBackHub.Methods.ExamPracticeWritingAIFeedBack, message);
            }
        }
    }
}
