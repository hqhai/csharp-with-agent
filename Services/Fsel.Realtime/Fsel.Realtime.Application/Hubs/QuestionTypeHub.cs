// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Realtime.Application.Hubs
{
    using Fsel.Core.Base;
    using Fsel.Core.Services.IpApiServices;
    using Fsel.Realtime.Application.Queues.Publishers;
    using Fsel.Shared.Models.ShareModels;
    using Microsoft.AspNetCore.Http;

    public class QuestionTypeHub : BaseHub
    {
        private readonly QuestionTypePublisher _questionTypePublisher;

        public QuestionTypeHub(AuthContext authContext, IIpApiService ipApiService, IHttpContextAccessor httpContextAccessor, QuestionTypePublisher tracingQuestionTypePublisher) : base(authContext, ipApiService, httpContextAccessor)
        {
            _questionTypePublisher = tracingQuestionTypePublisher;
        }

        public async Task SaveResult(QuestionResultQueueModel model)
        {
            await _questionTypePublisher.Publish(model, CancellationToken.None);
        }
    }
}
