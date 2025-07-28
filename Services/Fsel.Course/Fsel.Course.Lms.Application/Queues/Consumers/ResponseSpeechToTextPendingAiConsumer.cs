// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queues.Consumers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Fsel.Course.Lms.Application.Commands.ClassForumCmd.V1i1;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class ResponseSpeechToTextPendingAiConsumer : BaseConsumer<ResponseSpeechToTextPendingAiConsumerModel>
    {
        private readonly IMediator _mediator;

        public ResponseSpeechToTextPendingAiConsumer(AuthContext authContext, IHttpContextAccessor httpContextAccessor, IMediator mediator) : base(authContext, httpContextAccessor)
        {
            _mediator = mediator;
        }

        public async override Task ConsumeQueue(ResponseSpeechToTextPendingAiConsumerModel? message)
        {
            if (message == null)
            {
                return;
            }

            await _mediator.Send(new UpdateCFRPendingWordContentCommand
            {
                ClassForumDetailResultId = message.ClassForumDetailResultId,
                WordContent = message.WordContent
            });
        }
    }
}
