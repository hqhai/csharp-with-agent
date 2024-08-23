// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queues.Consumers
{
    using Fsel.Course.Lms.Application.Commands.AiCmd;
    using MediatR;
    using Fsel.Course.Domain.Models.QueryModels.ClassForumAutoDot;
    using Fsel.Core.Base;

    public class AiFeedBackResponseConsumer : BaseConsumer<MockTestAnswerResponseModel>
    {
        private readonly IMediator _mediator;

        public AiFeedBackResponseConsumer(IMediator mediator, AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(MockTestAnswerResponseModel? message)
        {
            if (message == null)
            {
                return;
            }
            await _mediator.Send(new SubmitMockTestAnswerAICommand
            {
                SectionId = message.SectionId,
                WordContent = message.WordContent,
                MockTestResultId = message.MockTestResultId,
                SectionGroupId = message.SectionGroupId
            }).ConfigureAwait(false);
        }
    }
}
