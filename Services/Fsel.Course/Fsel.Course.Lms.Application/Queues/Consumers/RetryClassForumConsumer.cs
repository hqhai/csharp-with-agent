// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queues.Consumers
{
    using Fsel.Core.Base;
    using Fsel.Course.Lms.Application.Commands.AiCmd.V1i2;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;

    public class RetryClassForumConsumer : BaseConsumer<SetTimeRetryClassForumModel>
    {
        private readonly IMediator _mediator;

        public RetryClassForumConsumer(IMediator mediator, AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(SetTimeRetryClassForumModel? message)
        {

            await _mediator.Send(new SubmitClassForumAICommand
            {
                ClassForumResultId = message?.ClassForumResultId ?? default,
                ClassForumDetailResultId = message?.ClassForumDetailResultId ?? default,
                WordContent = message?.WordContent ?? default,
                IsRetry = message?.IsRetry ?? default,
                SubmissionCount = message?.SubmissionCount ?? default,
            }).ConfigureAwait(false);
        }
    }
}
