// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.SectionGroupCmd
{
    using System.Threading;
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Commands.FinalTestAnswerV1i1Cmd;
    using Fsel.Course.Lms.Application.Commands.MockTestAnswerV1i1Cmd;
    using Fsel.Course.Lms.Application.Commands.PlacementTestAnswerV1i1Cmd;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateSectionGroupByResultIdCommand : CompleteTestWhenTimeOutModel, IRequest<MethodResult<bool>>
    {
    }

    public class UpdateSectionGroupByObjectResultCommandHandler : IRequestHandler<UpdateSectionGroupByResultIdCommand, MethodResult<bool>>
    {
        private readonly ISectionGroupResultRepository _sectionGroupResultRepository;
        private readonly IMediator _mediator;

        public UpdateSectionGroupByObjectResultCommandHandler(ISectionGroupResultRepository sectionGroupResultRepository, IMediator mediator)
        {
            _sectionGroupResultRepository = sectionGroupResultRepository;
            _mediator = mediator;
        }

        public async Task<MethodResult<bool>> Handle(UpdateSectionGroupByResultIdCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();
            var sectionGroupResult = await _sectionGroupResultRepository.Queryable.Include(x => x.SectionGroup).Where(x => x.Id == request.ObjectResultId).FirstOrDefaultAsync(cancellationToken);
            var sectionGroup = sectionGroupResult?.SectionGroup;
            if ((sectionGroupResult == null || sectionGroup == null) || sectionGroupResult.Status == EnumResultStatus.Done)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            var objectResultId = sectionGroupResult.FinalTestResultId ?? sectionGroupResult.MockTestResultId ?? sectionGroupResult.PlacementTestResultId ?? default;
            if (request.ObjectResultType == nameof(FinalTest))
            {
                await _mediator.Send(new CreateFinalTestAnswerBySectionGroupCommand { FinalTestResultId = objectResultId, IsSubmit = true, SectionGroupId = sectionGroupResult.SectionGroupId, StudentId = sectionGroupResult.StudentId }, cancellationToken).ConfigureAwait(false);
            }
            else if (request.ObjectResultType == nameof(MockTest))
            {
                await _mediator.Send(new CreateMockTestAnswerBySectionGroupCommand { MockTestResultId = objectResultId, IsSubmit = true, SectionGroupId = sectionGroupResult.SectionGroupId, StudentId = sectionGroupResult.StudentId }, cancellationToken).ConfigureAwait(false);
            }
            else if (request.ObjectResultType == nameof(PlacementTest))
            {
                await _mediator.Send(new CreatePlacementTestAnswerBySectionGroupCommand { PlacementTestResultId = objectResultId, IsSubmit = true, SectionGroupId = sectionGroupResult.SectionGroupId, StudentId = sectionGroupResult.StudentId }, cancellationToken).ConfigureAwait(false);
            }
            return methodResult;
        }
    }
}
