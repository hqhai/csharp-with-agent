// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queues.Consumers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Fsel.Identity.Application.Commands.StudentCmd;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;

    public class WeeklyNoticeConsumer : BaseConsumer<WeeklyNoticeQueueModel>
    {
        private readonly IMediator _mediator;

        public WeeklyNoticeConsumer(IMediator mediator, AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(WeeklyNoticeQueueModel? message)
        {
            if (message == null)
            {
                return;
            }
            await _mediator.Send(new NoticeExtendCommand()
            ).ConfigureAwait(false);
        }
    }
}
