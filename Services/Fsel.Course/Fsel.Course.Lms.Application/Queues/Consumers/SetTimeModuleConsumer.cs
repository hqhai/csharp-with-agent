// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queues.Consumers
{
    using Fsel.Core.Base;
    using Fsel.Course.Lms.Application.Commands.OtherFeatureCmd;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;

    public class SetTimeModuleConsumer : BaseConsumer<SetTimeModuleModel>
    {
        private readonly IMediator _mediator;

        public SetTimeModuleConsumer(IMediator mediator, AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(SetTimeModuleModel? message)
        {
            if (message == null)
            {
                return;
            }

            await _mediator.Send(new SetTimeModuleCommand { AccessTime = message.AccessTime, ObjectId = message.ObjectId, Type = message.Type, SubmissionCount = message.SubmissionCount }).ConfigureAwait(false);
        }
    }
}
