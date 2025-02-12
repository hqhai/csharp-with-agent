// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queues.Consumers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Models.CommandModels.PlacementTestAnswers;
    using Fsel.Course.Lms.Application.Commands.PlacementTestCmd.V1i1;
    using Fsel.Course.Lms.Application.Queries.Reports;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;

    public class SavePlacementTestAnswersConsumer : BaseConsumer<CreatePlacementTestAnswerBySectionGroupCommandModel>
    {
        private readonly IMediator _mediator;

        public SavePlacementTestAnswersConsumer(IMediator mediator, AuthContext authContext, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor) : base(authContext, httpContextAccessor)
        {
            _mediator = mediator;
        }

        public override async Task ConsumeQueue(CreatePlacementTestAnswerBySectionGroupCommandModel? message)
        {
            if (message == null)
            {
                return;
            }

            await _mediator.Send(new SavePlacementTestAnswersCommand
            {
                IsSubmit = message.IsSubmit,
                Answers = message.Answers,
                PlacementTestResultId = message.PlacementTestResultId,
                SectionGroupId = message.SectionGroupId,
                StudentId = message.StudentId
            }).ConfigureAwait(false);
        }
    }
}
