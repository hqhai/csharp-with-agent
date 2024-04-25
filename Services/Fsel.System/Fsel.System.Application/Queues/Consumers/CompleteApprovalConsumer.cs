using Fsel.Core.Base.BaseModels;
using Fsel.Shared.Models.ShareModels;
using Fsel.System.Application.Commands.ApprovalLogCmd;
using MassTransit;
using MediatR;

namespace Fsel.System.Application.Queues.Consumers
{
    public class CompleteApprovalConsumer : Core.Base.Interfaces.IBaseConsumer<SetTimeCompleteApprovalModel>
    {
        private readonly IMediator _mediator;

        public CompleteApprovalConsumer(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Consume(ConsumeContext<BaseQueueDataModel<SetTimeCompleteApprovalModel>> context)
        {
            if (context != null)
            {
                {
                    await _mediator.Send(new CreateApprovalLogCommand
                    {
                        ObjectId = context.Message.Data.ObjectId,
                        StartDate = context.Message.Data.StartDate,
                        ApprovalType = context.Message.Data.ApprovalType,
                    }).ConfigureAwait(false);
                }
            }
        }
    }
}
