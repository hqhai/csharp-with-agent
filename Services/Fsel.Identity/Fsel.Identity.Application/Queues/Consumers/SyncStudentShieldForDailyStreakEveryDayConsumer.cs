using Fsel.Core.Base;
using Fsel.Core.Base.BaseModels;
using Fsel.Identity.Application.Commands.DailyStreakCmd;
using MassTransit;
using MediatR;

namespace Fsel.Identity.Application.Queues.Consumers
{
    public class SyncStudentShieldForDailyStreakEveryDayConsumer : BaseConsumer<BaseQueueModel>
    {
        private readonly IMediator _mediator;

        public SyncStudentShieldForDailyStreakEveryDayConsumer(IMediator mediator, AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(BaseQueueModel? message)
        {
            await _mediator.Send(new SyncStudentShieldEveryDayCommand()).ConfigureAwait(false);
        }
    }
}
