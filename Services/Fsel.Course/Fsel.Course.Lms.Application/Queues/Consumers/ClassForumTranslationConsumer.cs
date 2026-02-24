// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queues.Consumers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Fsel.Course.Lms.Application.Commands.AiCmd;
    using Fsel.Course.Lms.Application.Queues.Models;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    public class ClassForumTranslationConsumer : BaseConsumer<ClassForumTranslationQueueModel>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ClassForumTranslationConsumer> _logger;

        public ClassForumTranslationConsumer(IMediator mediator,
                                            AuthContext authContext,
                                            IHttpContextAccessor httpContextAccessor,
                                            ILogger<ClassForumTranslationConsumer> logger) : base(authContext, httpContextAccessor)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public override async Task ConsumeQueue(ClassForumTranslationQueueModel? message)
        {
            if (IsNullMessage(message))
            {
                return;
            }

            await ExecuteTranslationCommandAsync(message!);
        }

        #region Private Methods

        private bool IsNullMessage(ClassForumTranslationQueueModel? message)
        {
            return message == null;
        }

        private async Task ExecuteTranslationCommandAsync(ClassForumTranslationQueueModel message)
        {
            try
            {
                _logger.LogInformation($"Processing ClassForumTranslationRequest for Id: {message.ClassForumDetailResultId}");

                await _mediator.Send(new SubmitTranslationAICommand
                {
                    ClassForumDetailResultId = message.ClassForumDetailResultId,
                    GradingAlFeedback = message.GradingAlFeedback
                }, CancellationToken.None);

                _logger.LogInformation($"Translation completed for Id: {message.ClassForumDetailResultId}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing ClassForumTranslationRequest for Id: {message.ClassForumDetailResultId}, Error: {ex.Message}");
                throw;
            }
        }
         
        #endregion
    }
}
