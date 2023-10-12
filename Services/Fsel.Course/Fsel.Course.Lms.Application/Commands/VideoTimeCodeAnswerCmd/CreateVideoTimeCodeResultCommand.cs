// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.VideoTimeCodeAnswerCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class CreateVideoTimeCodeResultCommand : IRequest<MethodResult<bool>>
    {
        public Guid VideoResultId { get; set; }
        public Guid StudentId { get; set; }
        public Guid VideoTimeCodeId { get; set; }
    }

    public class CreateVideoTimeCodeResultCommandHandler : IRequestHandler<CreateVideoTimeCodeResultCommand, MethodResult<bool>>
    {
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly IVideoTimeCodeResultRepository _videoTimeCodeResultRepository;

        public CreateVideoTimeCodeResultCommandHandler(
            IVideoResultRepository videoResultRepository
            , IVideoTimeCodeResultRepository videoTimeCodeResultRepository)
        {
            _videoResultRepository = videoResultRepository;
            _videoTimeCodeResultRepository = videoTimeCodeResultRepository;
        }

        public async Task<MethodResult<bool>> Handle(CreateVideoTimeCodeResultCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<bool> methodResult = new MethodResult<bool>();
            var videoResult = await _videoResultRepository.Queryable.FirstOrDefaultAsync(x => x.Id == request.VideoResultId, cancellationToken);
            if (videoResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(videoResult));
                return methodResult;
            }
            await GetAndUpdateVideoTimeCodeResultAsync(request);
            methodResult.Result = true;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<VideoTimeCodeResult> GetAndUpdateVideoTimeCodeResultAsync(CreateVideoTimeCodeResultCommand request)
        {
            var videoTimeCodeResult = await _videoTimeCodeResultRepository.Queryable.Where(x => x.VideoTimeCodeId == request.VideoTimeCodeId && x.VideoResultId == request.VideoResultId).FirstOrDefaultAsync();
            if (videoTimeCodeResult == null)
            {
                videoTimeCodeResult = new VideoTimeCodeResult
                {
                    VideoResultId = request.VideoResultId,
                    StudentId = request.StudentId,
                    VideoTimeCodeId = request.VideoTimeCodeId,
                };
                videoTimeCodeResult = _videoTimeCodeResultRepository.Add(videoTimeCodeResult);
                await _videoTimeCodeResultRepository.UnitOfWork.SaveEntitiesAsync().ConfigureAwait(false);
            }
            return videoTimeCodeResult;
        }
    }
}
