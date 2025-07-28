// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queues.Consumers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Models.CommandModels.QuestionExplanationErrors;
    using Fsel.Course.Lms.Application.Commands.QuestionExplanationErrorCmd;
    using MediatR;

    public class ErrorExplainConsumer : BaseConsumer<CreateQuestionExplanationErrorCommandModel>
    {
        private readonly IMediator _mediator;

        public ErrorExplainConsumer(IMediator mediator, AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(CreateQuestionExplanationErrorCommandModel? message)
        {
            if (message == null)
            {
                return;
            }

            await _mediator.Send(new CreateQuestionExplanationErrorByQueueCommand
            {
                Feedback = message.Feedback,
                FeedbackExplanation = message.FeedbackExplanation,
                QuestionId = message.QuestionId,
                ExplanationType = message.ExplanationType,
                ObjectResultId = message.ObjectResultId
            }).ConfigureAwait(false);
        }
    }
}
