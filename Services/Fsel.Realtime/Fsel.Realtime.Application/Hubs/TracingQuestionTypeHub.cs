// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Realtime.Application.Hubs
{
    using Fsel.Core.Base;
    using Fsel.Core.Services.IpApiServices;
    using Fsel.Realtime.Application.Queues.Publishers;
    using Fsel.Shared.Models;
    using Microsoft.AspNetCore.Http;

    public class TracingQuestionTypeHub : BaseHub
    {
        private readonly TracingQuestionTypePublisher _tracingQuestionTypePublisher;

        public TracingQuestionTypeHub(AuthContext authContext, IIpApiService ipApiService, IHttpContextAccessor httpContextAccessor, TracingQuestionTypePublisher tracingQuestionTypePublisher) : base(authContext, ipApiService, httpContextAccessor)
        {
            _tracingQuestionTypePublisher = tracingQuestionTypePublisher;
        }

        public async Task SaveResult(QuestionResultQueueModel model)
        {
            await _tracingQuestionTypePublisher.Publish(model, CancellationToken.None);
        }
    }
}
