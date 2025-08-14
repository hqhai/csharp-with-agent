namespace Fsel.Course.Lms.Application.Queues.Consumers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Fsel.Course.Lms.Application.Commands.QuestionCmd;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class QuestionTypeConsumer : BaseConsumer<QuestionResultQueueModel>
    {
        private readonly IMediator _mediator;

        public QuestionTypeConsumer(AuthContext authContext, IHttpContextAccessor httpContextAccessor, IMediator mediator) : base(authContext, httpContextAccessor)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(QuestionResultQueueModel? message)
        {
            if (message != null)
            {
                await _mediator.Send(new QuestionTypeHandlerCommand
                {
                    TResultId = message.TResultId,
                    QuestionId = message.QuestionId,
                    Type = message.Type,
                    Config = message.Config,
                    QuestionType = message.QuestionType
                });
            }
        }
    }
}
