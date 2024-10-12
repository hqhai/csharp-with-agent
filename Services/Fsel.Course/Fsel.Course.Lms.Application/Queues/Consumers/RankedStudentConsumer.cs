// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queues.Consumers
{
    using Fsel.Core.Base;
    using Fsel.Course.Lms.Application.Commands.StudentRankingEventCmd;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;

    public class RankedStudentConsumer : BaseConsumer<StudentRankingEventModel>
    {
        private readonly IMediator _mediator;

        public RankedStudentConsumer(IMediator mediator, AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(StudentRankingEventModel? message)
        {
            if (message == null)
            {
                return;
            }
            await _mediator.Send(new SetStudentRankingEventCommand
            {
                UserId = message.UserId
            }).ConfigureAwait(false);
        }
    }
}
