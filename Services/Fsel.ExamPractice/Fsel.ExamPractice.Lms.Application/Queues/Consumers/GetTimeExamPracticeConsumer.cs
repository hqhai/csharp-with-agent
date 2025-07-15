// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Lms.Application.Queues.Consumers
{
    using Fsel.Core.Base;
    using Fsel.ExamPractice.Lms.Application.Queries.OtherQuery;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;

    public class GetTimeExamPracticeConsumer : BaseConsumer<SetTimeExamPracticeModel>
    {
        private readonly IMediator _mediator;

        public GetTimeExamPracticeConsumer(IMediator mediator, AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(SetTimeExamPracticeModel? message)
        {
            if (message == null)
            {
                return;
            }
            await _mediator.Send(new GetTimeExamPracticeQuery { AccessTime = message.AccessTime, ObjectId = message.ObjectId, Type = message.Type }).ConfigureAwait(false);
        }
    }
}
