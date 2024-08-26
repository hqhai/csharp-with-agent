// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queues.Consumers
{
    using Fsel.Core.Base;
    using Fsel.Identity.Application.Commands.UserReferrals;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;

    public class AddFeatureMissionConsumer : BaseConsumer<AddFeatureMissionQueueModel>
    {
        private readonly IMediator _mediator;

        public AddFeatureMissionConsumer(IMediator mediator, AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(AddFeatureMissionQueueModel? message)
        {
            if (message == null)
            {
                return;
            }

            await _mediator.Send(new AddFeatureMissionCommand
            {
                FeatureUserReferral = message.FeatureUserReferral,
                ReceiverId = message.ReceiverId,
                Token = message.Token,
                PackageId = message.PackageId,
            }).ConfigureAwait(false);
        }
    }
}
