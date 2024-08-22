// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base;
using Fsel.Identity.Application.Commands.StudentRankingEvents;
using Fsel.Shared.Models.ShareModels;
using MediatR;

namespace Fsel.Identity.Application.Queues.Consumers
{
    public class StudentRankingEventsConsumer : BaseConsumer<StudentRankingEventModel>
    {
        private readonly IMediator _mediator;

        public StudentRankingEventsConsumer(IMediator mediator, AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(StudentRankingEventModel? message)
        {
            if (message != null)
            {
                await _mediator.Send(new CreateStudentRankingEventCommand
                {
                    RankingScore = message.RankingScore,
                    Process = message.Proccess,
                    UserId = message.UserId,
                    StudentId = message.StudentId,
                    CourseResultId = message.CourseResultId,
                    OverallScore = message.OverallScore
                }).ConfigureAwait(false);
            }
        }
    }
}
