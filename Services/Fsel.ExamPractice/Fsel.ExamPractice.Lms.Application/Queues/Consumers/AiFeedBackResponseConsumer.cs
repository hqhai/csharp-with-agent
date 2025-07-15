// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Lms.Application.Queues.Consumers
{
    using Fsel.Core.Base;
    using Fsel.Course.Lms.Application.Commands.AiCmd;
    using Fsel.ExamPractice.Domain.Models.EntityModels.ExamPracticeAnswers;
    using MediatR;

    public class AiFeedBackResponseConsumer : BaseConsumer<ExamPracticeAnswerResponseModel>
    {
        private readonly IMediator _mediator;

        public AiFeedBackResponseConsumer(IMediator mediator, AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(ExamPracticeAnswerResponseModel? message)
        {
            if (message == null)
            {
                return;
            }
            await _mediator.Send(new SubmitExamPracticeAnswerAICommand
            {
                ExamPracticeSectionId = message.ExamPracticeSectionId,
                WordContent = message.WordContent,
                ExamPracticeSectionResultId = message.ExamPracticeSectionResultId,
                IsRetry = message.IsRetry
            }).ConfigureAwait(false);
        }
    }
}
