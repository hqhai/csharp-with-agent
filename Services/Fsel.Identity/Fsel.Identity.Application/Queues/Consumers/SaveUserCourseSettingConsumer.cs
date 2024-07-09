// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queues.Consumers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Fsel.Identity.Application.Commands.UserCourseSettingCmd;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;

    public class SaveUserCourseSettingConsumer : BaseConsumer<SaveUserCourseSettingQueueModel>
    {
        private readonly IMediator _mediator;

        public SaveUserCourseSettingConsumer(IMediator mediator, AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(SaveUserCourseSettingQueueModel? message)
        {
            if (message == null)
            {
                return;
            }

            await _mediator.Send(new SaveUserCourseSettingCommand
            {
                UserId = message.UserId,
                Type = message.Type,
                CourseLevel = message.CourseLevel,
                IsDeduction = message.IsDeduction
            }).ConfigureAwait(false);
        }
    }
}
