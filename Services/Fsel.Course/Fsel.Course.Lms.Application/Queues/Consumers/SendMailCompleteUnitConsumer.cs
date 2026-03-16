// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queues.Consumers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Fsel.Course.Lms.Application.Commands.OtherCmd;
    using MediatR;

    public class SendMailCompleteUnitConsumer : BaseConsumer<SendMailCompleteUnitCommandModel>
    {
        private readonly IMediator _mediator;

        public SendMailCompleteUnitConsumer(IMediator mediator, AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(SendMailCompleteUnitCommandModel? message)
        {
            if (message == null)
            {
                return;
            }

            await _mediator.Send(new SendMailCompleteUnitCommand
            {
                UnitResultId = message.UnitResultId,
            }).ConfigureAwait(false);
        }
    }
}
