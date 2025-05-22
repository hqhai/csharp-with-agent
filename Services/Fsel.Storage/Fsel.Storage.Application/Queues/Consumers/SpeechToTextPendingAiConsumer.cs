// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Storage.Application.Queues.Consumers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.Storage.Application.Command.SpeechToTextCmd.V1i2;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class SpeechToTextPendingAiConsumer : BaseConsumer<SpeechToTextPendingAiConsumerModel>
    {
        private readonly IMediator _mediator;

        public SpeechToTextPendingAiConsumer(AuthContext authContext, IHttpContextAccessor httpContextAccessor, IMediator mediator) : base(authContext, httpContextAccessor)
        {
            _mediator = mediator;
        }

        public async override Task ConsumeQueue(SpeechToTextPendingAiConsumerModel? message)
        {
            if (message == null)
            {
                return;
            }

            await _mediator.Send(new ConvertSpeechToTextPendingCommand
            {
                ClassForumDetailResultId = message.ClassForumDetailResultId,
                ContentType = message.ContentType,
                FileData = message.FileData,
                FileName = message.FileName
            });
        }
    }
}
