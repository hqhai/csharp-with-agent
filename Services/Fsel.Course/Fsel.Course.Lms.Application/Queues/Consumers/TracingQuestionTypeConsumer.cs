namespace Fsel.Course.Lms.Application.Queues.Consumers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Fsel.Course.Lms.Application.Commands.QuestionCmd;
    using Fsel.Shared.Models;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class TracingQuestionTypeConsumer : BaseConsumer<QuestionResultQueueModel>
    {
        private readonly IMediator _mediator;

        public TracingQuestionTypeConsumer(AuthContext authContext, IHttpContextAccessor httpContextAccessor, IMediator mediator) : base(authContext, httpContextAccessor)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(QuestionResultQueueModel? message)
        {
            if (message != null)
            {
                await _mediator.Send(new TracingQuestionTypeHandlerCommand
                {
                    TResultId = message.TResultId,
                    QuestionId = message.QuestionId,
                    Type = message.Type,
                    CountFail = message.CountFail,
                    CountStrokes = message.CountStrokes
                });
            }
        }
    }
}
