namespace Fsel.ExamPractice.Lms.Application.Queues.Consumers
{
    using Fsel.Core.Base;
    using Fsel.ExamPractice.Lms.Application.Commands.ExamPracticeCmd;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;

    public class SetTimeExamPracticeConsumer : BaseConsumer<SetTimeModuleModel>
    {
        private readonly IMediator _mediator;

        public SetTimeExamPracticeConsumer(IMediator mediator, AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(SetTimeModuleModel? message)
        {
            if (message == null)
            {
                return;
            }

            await _mediator.Send(new SetTimeExamPracticeCommand { AccessTime = message.AccessTime, ObjectId = message.ObjectId, Type = message.Type, SubmissionCount = message.SubmissionCount }).ConfigureAwait(false);
        }
    }
}
