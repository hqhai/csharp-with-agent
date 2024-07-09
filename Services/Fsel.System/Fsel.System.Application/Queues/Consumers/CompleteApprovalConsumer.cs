using Fsel.Core.Base;
using Fsel.Core.Base.BaseModels;
using Fsel.Shared.Models.ShareModels;
using Fsel.System.Application.Commands.ApprovalLogCmd;
using MassTransit;
using MediatR;

namespace Fsel.System.Application.Queues.Consumers
{
    public class CompleteApprovalConsumer : BaseConsumer<SetTimeCompleteApprovalModel>
    {
        private readonly IMediator _mediator;

        public CompleteApprovalConsumer(IMediator mediator, AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(SetTimeCompleteApprovalModel? message)
        {
            if (message != null)
            {
                {
                    await _mediator.Send(new CreateApprovalLogCommand
                    {
                        ObjectId = message.ObjectId,
                        StartDate = message.StartDate,
                        ApprovalType = message.ApprovalType,
                    }).ConfigureAwait(false);
                }
            }
        }
    }
}
