// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.VideoTimeCodeResultCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class ActionVideoTimeCodeCommand : IRequest<MethodResult<VideoResultModel>>
    {
        public Guid VideoResultId { get; set; }
        public Guid VideoTimeCodeId { get; set; }
    }

    public class ActionVideoTimeCodeCommandHandler : IRequestHandler<ActionVideoTimeCodeCommand, MethodResult<VideoResultModel>>
    {
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly IVideoTimeCodeResultRepository _videoTimeCodeResultRepository;
        private readonly IVideoTimeCodeRepository _videoTimeCodeRepository;
        private readonly IMapper _mapper;

        public ActionVideoTimeCodeCommandHandler(IVideoResultRepository videoResultRepository,
            IVideoTimeCodeResultRepository videoTimeCodeResultRepository,
            IVideoTimeCodeRepository videoTimeCodeRepository,
            IMapper mapper)
        {
            _videoResultRepository = videoResultRepository;
            _videoTimeCodeResultRepository = videoTimeCodeResultRepository;
            _videoTimeCodeRepository = videoTimeCodeRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<VideoResultModel>> Handle(ActionVideoTimeCodeCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<VideoResultModel> methodResult = new MethodResult<VideoResultModel>();

            var videoResult = await _videoResultRepository.GetByIdAsync(request.VideoResultId);
            if (videoResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(videoResult));
                return methodResult;
            }
            var videoTimeCode = await _videoTimeCodeRepository.GetByIdAsync(request.VideoTimeCodeId);
            if (videoTimeCode == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(videoTimeCode));
                return methodResult;
            }
            var videoTimeCodeResult = await _videoTimeCodeResultRepository.Queryable.FirstOrDefaultAsync(x => x.VideoResultId == request.VideoResultId && x.VideoTimeCodeId == request.VideoTimeCodeId, cancellationToken);
            if (videoTimeCodeResult == null)
            {
                videoResult.CurrentVideoTimeCodeId = request.VideoTimeCodeId;
                videoResult = _videoResultRepository.Update(videoResult);
                await _videoResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            methodResult.Result = _mapper.Map<VideoResultModel>(videoResult);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
