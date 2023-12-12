using Fsel.Shared.Models.ShareModels;
using Fsel.System.Application.Commands.ApprovalLogCmd;
using MassTransit;
using MediatR;

namespace Fsel.System.Application.Queues.Consumers
{
    public class CompleteApprovalConsumer : IConsumer<SetTimeCompleteApprovalModel>
    {
        private readonly IMediator _mediator;

        public CompleteApprovalConsumer(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Consume(ConsumeContext<SetTimeCompleteApprovalModel> context)
        {
            if (context != null)
            {
                {
                    await _mediator.Send(new CreateApprovalLogCommand
                    {
                        ObjectId = context.Message.ObjectId,
                        ExpiredDate = context.Message.ExpiredDate,
                        ApprovalType = context.Message.ApprovalType,
                    }).ConfigureAwait(false);
                }
            }
        }
    }
}
