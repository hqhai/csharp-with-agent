// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Interaction.Application.Queues.Consumers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Fsel.Interaction.Application.Commands.CustomerSurveyCmd;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;

    public class SendNotifyUserHasSurveyConsumer : BaseConsumer<SaveUserSurveyAssignmentCommandModel>
    {
        private readonly IMediator _mediator;

        public SendNotifyUserHasSurveyConsumer(IMediator mediator, AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(SaveUserSurveyAssignmentCommandModel? message)
        {
            if (message == null)
            {
                return;
            }
            await _mediator.Send(new SendNotifyUserHasSurveyCommand() { CourseLevel = message.CourseLevel, CourseType = message.CourseType, ProgressRequirement = message.ProgressRequirement }).ConfigureAwait(false);
        }
    }
}
