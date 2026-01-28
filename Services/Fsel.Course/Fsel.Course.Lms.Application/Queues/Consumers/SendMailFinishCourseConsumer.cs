// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queues.Consumers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Fsel.Course.Lms.Application.Commands.OtherCmd;
    using MediatR;

    public class SendMailFinishCourseConsumer : BaseConsumer<SendMailFinishCourseModel>
    {
        private readonly IMediator _mediator;

        public SendMailFinishCourseConsumer(IMediator mediator, AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(SendMailFinishCourseModel? message)
        {
            if (message == null)
            {
                return;
            }

            await _mediator.Send(new SendMailFinishCourseCommand
            {
                StudentId = message.StudentId,
                CourseId = message.CourseId,
            }).ConfigureAwait(false);
        }
    }
}
