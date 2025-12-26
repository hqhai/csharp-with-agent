// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queues.Consumers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Fsel.Course.Lms.Application.Commands.AiCmd;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class ClassForumPronunciationConsumer : BaseConsumer<ClassForumPronunciationConsumerModel>
    {
        private readonly IMediator _mediator;

        public ClassForumPronunciationConsumer(AuthContext authContext, IHttpContextAccessor httpContextAccessor, IMediator mediator) : base(authContext, httpContextAccessor)
        {
            _mediator = mediator;
        }

        public async override Task ConsumeQueue(ClassForumPronunciationConsumerModel? message)
        {
            if (message == null)
            {
                return;
            }

            await _mediator.Send(new ClassForumPronunciationCommand { ClassForumDetailResultId = message.ClassForumDetailResultId }).ConfigureAwait(false);

        }
    }
}
