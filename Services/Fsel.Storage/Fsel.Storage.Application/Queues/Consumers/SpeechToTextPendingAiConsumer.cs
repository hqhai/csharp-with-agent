// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Storage.Application.Queues.Consumers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.Storage.Application.Command.SpeechToTextCmd.V1i2;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;

    public class SpeechToTextPendingAiConsumer : BaseConsumer<SpeechToTextPendingAiConsumerModel>
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public SpeechToTextPendingAiConsumer(AuthContext authContext, IHttpContextAccessor httpContextAccessor, IServiceScopeFactory serviceScopeFactory) : base(authContext, httpContextAccessor)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async override Task ConsumeQueue(SpeechToTextPendingAiConsumerModel? message)
        {
            if (message == null)
            {
                return;
            }

            _ = Task.Run(async () =>
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                await mediator.Send(new ConvertSpeechToTextPendingCommand
                {
                    UserId = message.UserId,
                    ClassForumDetailResultId = message.ClassForumDetailResultId,
                    ContentType = message.ContentType,
                    FileData = message.FileData,
                    FileName = message.FileName
                });
            });
        }
    }
}
