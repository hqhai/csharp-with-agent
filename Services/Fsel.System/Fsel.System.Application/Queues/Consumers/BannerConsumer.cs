// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queues.Consumers
{
    using Fsel.Core.Base;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.System.Application.Queries.BannerQuery;
    using MediatR;

    public class BannerConsumer : BaseConsumer<BannerMessageModel>
    {
        private readonly IMediator _mediator;

        public BannerConsumer(IMediator mediator, AuthContext authContext) : base(authContext)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(BannerMessageModel? message)
        {
            if (message == null)
            {
                return;
            }

            await _mediator.Send(new GetBannerByStudentQuery
            {
                Date = message.Date,
                UserId = message.UserId
            }).ConfigureAwait(false);
        }
    }
}
