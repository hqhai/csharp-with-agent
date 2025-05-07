// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.VideoTimeCodeAnswerCmd.V1i1
{
    using Fsel.Common.ActionResults;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class UpdateVideoTimeCodeByResultIdCommand : CompleteTestWhenTimeOutModel, IRequest<MethodResult<bool>>
    {
    }

    public class UpdateVideoTimeCodeByResultIdCommandHandler : IRequestHandler<UpdateVideoTimeCodeByResultIdCommand, MethodResult<bool>>
    {
        private readonly IVideoTimeCodeResultRepository _videoTimeCodeResultRepository;
        private readonly IMediator _mediator;

        public UpdateVideoTimeCodeByResultIdCommandHandler(IVideoTimeCodeResultRepository videoTimeCodeResultRepository, IMediator mediator)
        {
            _videoTimeCodeResultRepository = videoTimeCodeResultRepository;
            _mediator = mediator;
        }

        public async Task<MethodResult<bool>> Handle(UpdateVideoTimeCodeByResultIdCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();
            var videoTimeCodeResult = await _videoTimeCodeResultRepository.Queryable.Include(x => x.VideoTimeCode).Where(x => x.Id == request.ObjectResultId).FirstOrDefaultAsync(cancellationToken);
            var videoTimeCode = videoTimeCodeResult?.VideoTimeCode;
            if (videoTimeCodeResult == null || videoTimeCode == null || videoTimeCodeResult.Status == EnumResultStatus.Done)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            var objectResultId = videoTimeCodeResult.VideoTimeCodeId;
            await _mediator.Send(new CreateVideoTimeCodeAnswerByTimeCodeCommand { VideoTimeCodeId = objectResultId, IsSubmit = true, VideoResultId = videoTimeCodeResult.VideoResultId, StudentId = videoTimeCodeResult.StudentId }, cancellationToken).ConfigureAwait(false);
            return methodResult;
        }
    }
}
