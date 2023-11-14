// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queues.Consumers
{
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Infrastructure.Migrations;
    using Fsel.Course.Lms.Application.Commands.SectionGroupCmd;
    using Fsel.Course.Lms.Application.Commands.VideoTimeCodeCmd;
    using Fsel.Shared.Models.ShareModels;
    using MassTransit;
    using MediatR;

    public class CompleteTestWhenTimeOutConsumer : IConsumer<CompleteTestWhenTimeOutModel>
    {
        private readonly IMediator _mediator;

        public CompleteTestWhenTimeOutConsumer(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Consume(ConsumeContext<CompleteTestWhenTimeOutModel> context)
        {
            if (context == null)
            {
                return;
            }
            var messeger = context.Message;
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
