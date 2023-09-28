using Fsel.Identity.Application.Commands.StudentCmd;
using Fsel.Shared.Models.ShareModels;
using MassTransit;
using MediatR;

namespace Fsel.Identity.Application.Queues.Consumers
{
    public class UpdateStudentsDailyStreakConsumer : IConsumer<CreateStudentDailyStreakQueueModel>
    {
        private readonly IMediator _mediator;

        public UpdateStudentsDailyStreakConsumer(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Consume(ConsumeContext<CreateStudentDailyStreakQueueModel> context)
        {
            if (context == null)
            {
                return;
            }
            var message = context.Message;
            await _mediator.Send(new CreateStudentDailyStreakCommand { IsUseShield = message.IsUseShield, StudentId = message.StudentId }).ConfigureAwait(false);
        }
    }
}
