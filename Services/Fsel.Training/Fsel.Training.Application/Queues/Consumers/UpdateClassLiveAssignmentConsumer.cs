using Fsel.Core.Base;
using Fsel.Core.Base.BaseModels;
using Fsel.Training.Application.Commands.ClassLiveCmd;
using MassTransit;
using MediatR;

namespace Fsel.Training.Application.Queues.Consumers
{
    public class UpdateClassLiveAssignmentConsumer : BaseConsumer<BaseQueueModel>
    {
        private readonly IMediator _mediator;

        public UpdateClassLiveAssignmentConsumer(IMediator mediator, AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(BaseQueueModel? message)
        {
            await _mediator.Send(new UpdateClassLiveAssignmentCommand()).ConfigureAwait(false);
        }
    }
}
