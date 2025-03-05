// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Storage.Application.Queues.Consumers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.Storage.Application.Command.SpeechToTextCmd.V1i2;
    using Fsel.Storage.Application.Queues.Publisher;
    using Fsel.Storage.Application.Services.AmazonS3Services;
    using Fsel.Storage.Application.Services.OpenAIServices;
    using Fsel.Storage.Domain.Models.EntityModels;
    using Fsel.Storage.Infrastructure.ValueSettings;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Polly;
    using Refit;

    public class SpeechToTextAiConsumer : BaseConsumer<SpeechToTextAiConsumerModel>
    {
        private readonly IMediator _mediator;


        public SpeechToTextAiConsumer(IMediator mediator, AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _mediator = mediator;
        }
        public async override Task ConsumeQueue(SpeechToTextAiConsumerModel? message)
        {
            if (message == null)
            {
                return;
            }

            await _mediator.Send(new PublishSpeakToTextToRealTimeCommand
            {
                UserId = message.UserId,
                FileName = message.FileName,
                ContentType = message.ContentType,
                FileData = message.FileData
            }).ConfigureAwait(false);
        }

    }
}
