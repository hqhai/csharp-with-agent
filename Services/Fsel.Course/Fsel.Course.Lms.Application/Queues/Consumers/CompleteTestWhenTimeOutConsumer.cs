// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queues.Consumers
{
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Lms.Application.Commands.SectionGroupCmd;
    using Fsel.Course.Lms.Application.Commands.VideoTimeCodeAnswerCmd.V1i1;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;

    public class CompleteTestWhenTimeOutConsumer : BaseConsumer<CompleteTestWhenTimeOutModel>
    {
        private readonly IMediator _mediator;

        public CompleteTestWhenTimeOutConsumer(IMediator mediator, AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(CompleteTestWhenTimeOutModel? message)
        {
            if (message == null)
            {
                return;
            }
            switch (message.ObjectResultType)
            {
                case nameof(MockTest):
                    await _mediator.Send(new UpdateSectionGroupByResultIdCommand { ObjectResultId = message.ObjectResultId, ObjectResultType = message.ObjectResultType }).ConfigureAwait(false);
                    break;

                case nameof(PlacementTest):
                    await _mediator.Send(new UpdateSectionGroupByResultIdCommand { ObjectResultId = message.ObjectResultId, ObjectResultType = message.ObjectResultType }).ConfigureAwait(false);
                    break;

                case nameof(FinalTest):
                    await _mediator.Send(new UpdateSectionGroupByResultIdCommand { ObjectResultId = message.ObjectResultId, ObjectResultType = message.ObjectResultType }).ConfigureAwait(false);
                    break;

                case nameof(EnumTimeCodeType.SkillTest):
                    await _mediator.Send(new UpdateVideoTimeCodeByResultIdCommand { ObjectResultId = message.ObjectResultId, ObjectResultType = message.ObjectResultType }).ConfigureAwait(false);
                    break;

                case nameof(EnumTimeCodeType.UnitTest):
                    await _mediator.Send(new UpdateVideoTimeCodeByResultIdCommand { ObjectResultId = message.ObjectResultId, ObjectResultType = message.ObjectResultType }).ConfigureAwait(false);
                    break;
            }
        }
    }
}
