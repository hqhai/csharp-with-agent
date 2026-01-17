// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queues.Consumers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Fsel.Course.Lms.Application.Commands.AiCmd;
    using MediatR;
    using Fsel.Shared.Models.ShareModels;
    using Microsoft.AspNetCore.Http;

    public class AITranslationResponseConsumer : BaseConsumer<AITranslationRequestModel>
    {
        private readonly IMediator _mediator;

        public AITranslationResponseConsumer(IMediator mediator,
                                             AuthContext authContext,
                                             IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(AITranslationRequestModel? message)
        {
            if (IsNullMessage(message))
            {
                return;
            }

            await ExecuteTranslationCommandAsync(message);
        }

        #region Private Methods

        private bool IsNullMessage(AITranslationRequestModel? message)
        {
            return message == null;
        }

        private async Task ExecuteTranslationCommandAsync(AITranslationRequestModel message)
        {
            await _mediator.Send(new SubmitTranslationAICommand
            {
                ClassForumDetailResultId = message.ClassForumDetailResultId,
                AiResponseContent = message.AiResponseContent
            }, CancellationToken.None);
        }

        #endregion
    }
}
