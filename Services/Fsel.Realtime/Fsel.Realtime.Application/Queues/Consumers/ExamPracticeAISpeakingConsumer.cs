using Fsel.Core.Base;
using Fsel.Core.Extensions;
using Fsel.Realtime.Application.Hubs;
using Fsel.Shared.Constants;
using Fsel.Shared.Models.ShareModels;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace Fsel.Realtime.Application.Queues.Consumers
{
    public class ExamPracticeAISpeakingConsumer : BaseConsumer<SubmitExamPracticeAiSpeakingResponseModel>
    {
        private readonly IHubContext<ExamPracticeSpeakingHub> _aISpeakingHub;

        public ExamPracticeAISpeakingConsumer(IHubContext<ExamPracticeSpeakingHub> aISpeakingHub, AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _aISpeakingHub = aISpeakingHub;
        }

        public override async Task ConsumeQueue(SubmitExamPracticeAiSpeakingResponseModel? message)
        {
            if (message != null)
            {
                var examPracticeResultId = message.ExamPracticeResultId.ToString();
                await _aISpeakingHub.GetGroup(examPracticeResultId!).SendAsync(RealtimeSettings.ExamPracticeSpeakingAIFeedBackHub.Methods.ExamPracticeSpeakingAIFeedBack, message);
            }
        }
    }
}
