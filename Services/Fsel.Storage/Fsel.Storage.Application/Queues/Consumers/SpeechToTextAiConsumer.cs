// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Storage.Application.Queues.Consumers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.Storage.Application.Command.SpeechToTextCmd.V1i2;
    using MediatR;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class SpeechToTextAiConsumer : BaseConsumer<SpeechToTextAiConsumerModel>
    {
        private readonly IMediator _mediator;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<SpeechToTextAiConsumer> _logger;

        public SpeechToTextAiConsumer(IMediator mediator, AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor, IServiceScopeFactory serviceScopeFactory, ILogger<SpeechToTextAiConsumer> logger) : base(authContext, httpContextAccessor)
        {
            _mediator = mediator;
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }
        public async override Task ConsumeQueue(SpeechToTextAiConsumerModel? message)
        {
            if (message == null)
            {
                return;
            }

            _ = Task.Run(async () =>
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                await mediator.Publish(new PublishSpeakToTextToRealTimeCommand
                {
                    UserId = message.UserId,
                    FileName = message.FileName,
                    ContentType = message.ContentType,
                    FileData = message.FileData,
                    CurrentDate = message.CurrentDate
                });
            });

        }

    }
}
