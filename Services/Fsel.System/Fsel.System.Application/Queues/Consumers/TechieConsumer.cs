using Fsel.Core.Base;
using Fsel.Shared.Models.ShareModels;
using Fsel.System.Application.Commands.TechieCmd;
using MediatR;

namespace Fsel.System.Application.Queues.Consumers
{
    public class TechieConsumer : BaseConsumer<StudentTechieActionModel>
    {
        private readonly IMediator _mediator;

        public TechieConsumer(IMediator mediator, AuthContext authContext) : base(authContext)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(StudentTechieActionModel? message)
        {
            if (message != null)
            {
                await _mediator.Send(new CreateStudentTechieCommand
                {
                    Config = message.Config,
                    Actions = message.Action,
                    TechieFeature = message.Feature,

                }).ConfigureAwait(false);
            }
        }
    }
}
