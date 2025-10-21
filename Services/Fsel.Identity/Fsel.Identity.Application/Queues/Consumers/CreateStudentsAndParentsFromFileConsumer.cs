// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queues.Consumers
{
    using Fsel.Core.Base;
    using Fsel.Identity.Application.Commands.StudentCmd;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;

    public class CreateStudentsAndParentsFromFileConsumer : BaseConsumer<CreateStudentsToEventFromByteModel>
    {
        private readonly IMediator _mediator;

        public CreateStudentsAndParentsFromFileConsumer(IMediator mediator, AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(CreateStudentsToEventFromByteModel? message)
        {
            if (message == null)
            {
                return;
            }

            var result = new CreateStudentsAndParentsToEventFromFileCommand
            {
                Category = message.Category,
                File = message.File,
                Key = message.Key
            };
            await _mediator.Send(result).ConfigureAwait(false);
        }
    }
}
