// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queues.Consumers
{
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Lms.Application.Commands.SectionGroupCmd;
    using Fsel.Course.Lms.Application.Commands.VideoTimeCodeAnswerCmd.V1i1;
    using Fsel.Shared.Models.ShareModels;
    using MassTransit;
    using MediatR;

    public class CompleteTestWhenTimeOutConsumer : BaseConsumer<CompleteTestWhenTimeOutModel>
    {
        private readonly IMediator _mediator;

        public CompleteTestWhenTimeOutConsumer(IMediator mediator, AuthContext authContext) : base(authContext)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(ConsumeContext<Core.Base.BaseModels.BaseQueueDataModel<CompleteTestWhenTimeOutModel>> context)
        {
            if (context == null)
            {
                return;
            }
            var messeger = context.Message?.Data;
            switch (messeger.ObjectResultType)
            {
                case nameof(MockTest):
                    await _mediator.Send(new UpdateSectionGroupByResultIdCommand { ObjectResultId = messeger.ObjectResultId, ObjectResultType = messeger.ObjectResultType }).ConfigureAwait(false);
                    break;

                case nameof(PlacementTest):
                    await _mediator.Send(new UpdateSectionGroupByResultIdCommand { ObjectResultId = messeger.ObjectResultId, ObjectResultType = messeger.ObjectResultType }).ConfigureAwait(false);
                    break;

                case nameof(FinalTest):
                    await _mediator.Send(new UpdateSectionGroupByResultIdCommand { ObjectResultId = messeger.ObjectResultId, ObjectResultType = messeger.ObjectResultType }).ConfigureAwait(false);
                    break;

                case nameof(EnumTimeCodeType.SkillTest):
                    await _mediator.Send(new UpdateVideoTimeCodeByResultIdCommand { ObjectResultId = messeger.ObjectResultId, ObjectResultType = messeger.ObjectResultType }).ConfigureAwait(false);
                    break;

                case nameof(EnumTimeCodeType.UnitTest):
                    await _mediator.Send(new UpdateVideoTimeCodeByResultIdCommand { ObjectResultId = messeger.ObjectResultId, ObjectResultType = messeger.ObjectResultType }).ConfigureAwait(false);
                    break;
            }
        }
    }
}
