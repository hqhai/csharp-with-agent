using Fsel.Shared.Models.ShareModels;
using Fsel.System.Application.Commands.ApprovalLogCmd;
using Fsel.System.Domain.Models.CommandModels.ApprovalLog;
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
                    IList<ApprovalLogCommandModel> contexts = new List<ApprovalLogCommandModel>();

                    contexts.Add(new ApprovalLogCommandModel()
                    {
                        ObjectId = context.Message.ObjectId,
                        ExpiredDate = context.Message.ExpiredDate,
                        ApprovalTimeConfigId = new Guid("D9255DC6-397F-47EA-A24D-6E8903276B09")

                    });

                    await _mediator.Send(new CreateApprovalLogCommand
                    {
                        ApprovalLogCommandModels = contexts
                    }).ConfigureAwait(false);
                }
            }
        }
    }
}
