using Fsel.Core.Base;
using Fsel.Core.Base.BaseModels;
using Fsel.Identity.Application.Commands.StudentCmd.StudentEventCmd;
using MediatR;

namespace Fsel.Identity.Application.Queues.Consumers
{
    public class AggregateDataStudentsInEventConsumer : BaseConsumer<BaseQueueModel>
    {
        private readonly IMediator _mediator;

        public AggregateDataStudentsInEventConsumer(IMediator mediator, AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _mediator = mediator;
        }

        public override Task ConsumeQueue(BaseQueueModel? message)
        {
            if (message == null)
            {
                return Task.CompletedTask;
            }

            _ = Task.Run(async () => await _mediator.Send(new AggregateDataStudentsInEventCommand()).ConfigureAwait(false));
            return Task.CompletedTask;
        }
    }
}
