// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queues.Consumers
{
    using Fsel.Core.Base;
    using Fsel.Course.Lms.Application.Queries.OtherFeatureQuery;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;

    public class GetTimeModuleConsumer : BaseConsumer<SetTimeModuleModel>
    {
        private readonly IMediator _mediator;

        public GetTimeModuleConsumer(IMediator mediator, AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(SetTimeModuleModel? message)
        {
            if (message == null)
            {
                return;
            }
            await _mediator.Send(new GetTimeModuleQuery { AccessTime = message.AccessTime, ObjectId = message.ObjectId, Type = message.Type }).ConfigureAwait(false);
        }
    }
}
