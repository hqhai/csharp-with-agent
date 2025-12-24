// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queues.Consumers
{
    using Commands.AiCmd;
    using MediatR;
    using Fsel.Course.Domain.Models.QueryModels.ClassForumAutoDot;
    using Fsel.Core.Base;

    public class RealTimeAIResponseConsumer : BaseConsumer<ClassForumAIResponseModel>
    {
        private readonly IMediator _mediator;

        public RealTimeAIResponseConsumer(IMediator mediator, AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext,
            httpContextAccessor)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(ClassForumAIResponseModel? message)
        {
            if (message == null)
            {
                return;
            }

            await _mediator.Send(new SubmitClassForumAiCommand
            {
                ClassForumResultId = message.ClassForumResultId,
                ClassForumDetailResultId = message.ClassForumDetailResultId,
                WordContent = message.WordContent,
                IsRetry = message.IsRetry,
                SubmissionCount = message.SubmissionCount,
            }).ConfigureAwait(false);
        }
    }
}
