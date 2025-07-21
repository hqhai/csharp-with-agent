// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queues.Consumers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Fsel.Identity.Application.Commands.StudentCmd;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;

    public class AddExpiredDateForStudentConsumer : BaseConsumer<AddExpiredDateForStudentQueueModel>
    {
        private readonly IMediator _mediator;

        public AddExpiredDateForStudentConsumer(IMediator mediator, AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(AddExpiredDateForStudentQueueModel? message)
        {
            if (message == null)
            {
                return;
            }

            await _mediator.Send(new AddExpiredDateForStudentCommand
            {
                StudentEditHistoryType = message.StudentEditHistoryType,
                StudentId = message.StudentId,
                Day = message.Day,
                Month = message.Month,
                ExpiredDate = message.ExpiredDate,
                Description = message.Description
            }).ConfigureAwait(false);
        }
    }
}
