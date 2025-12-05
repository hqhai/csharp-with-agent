// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.VideoQuery.V1i2
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Commands.VideoTimeCodeAnswerCmd;
    using Fsel.Course.Lms.Application.Services.ApplicationServices;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetTimeCodeDetailQuery : IRequest<MethodResult<VideoTimeCodeModel>>
    {
        public Guid VideoResultId { get; set; }
        public Guid VideoTimeCodeId { get; set; }
        public bool IsShowSubStatus { get; set; }
        public bool IsCreateAnswer { get; set; }
    }

    public class GetTimeCodeDetailQueryHandler : IRequestHandler<GetTimeCodeDetailQuery, MethodResult<VideoTimeCodeModel>>
    {
        private readonly IVideoTimeCodeRepository _videoTimeCodeRepository;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly IMediator _mediator;
        private readonly IVideoTimeCodeResultRepository _videoTimeCodeResultRepository;
        private readonly IVideoTimeCodeService _videoTimeCodeService;

        public GetTimeCodeDetailQueryHandler(IVideoTimeCodeRepository videoTimeCodeRepository,
            IVideoResultRepository videoResultRepository,
            IMediator mediator,
            IVideoTimeCodeResultRepository videoTimeCodeResultRepository,
            IVideoTimeCodeService videoTimeCodeService)
        {
            _videoTimeCodeRepository = videoTimeCodeRepository;
            _videoResultRepository = videoResultRepository;
            _mediator = mediator;
            _videoTimeCodeResultRepository = videoTimeCodeResultRepository;
            _videoTimeCodeService = videoTimeCodeService;
        }

        public async Task<MethodResult<VideoTimeCodeModel>> Handle(GetTimeCodeDetailQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<VideoTimeCodeModel> methodResult = new MethodResult<VideoTimeCodeModel>();

            var videoResult = await _videoResultRepository.ReadQueryable.Where(x => x.Id == request.VideoResultId).FirstOrDefaultAsync(cancellationToken);
            if (videoResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(videoResult));
                return methodResult;
            }
            var validateMethod = await Validate(request, videoResult, cancellationToken);
            if (videoResult.CurrentVideoTimeCodeId != request.VideoTimeCodeId && !validateMethod.IsOK)
            {
                methodResult.AddErrorBadRequest(validateMethod.ErrorMessages);
                return methodResult;
            }

            var videoTimeCode = await _videoTimeCodeRepository.GetByIdAsync(request.VideoTimeCodeId);
            if (videoTimeCode == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(videoTimeCode));
                return methodResult;
            }

            var method = await _mediator.Send(new CreateVideoTimeCodeResultCommand { VideoResultId = videoResult.Id, VideoTimeCodeId = request.VideoTimeCodeId, IsActive = !request.IsCreateAnswer }, cancellationToken);
            if (!method.IsOK)
            {
                methodResult.AddErrorBadRequest(method.ErrorMessages);
                return methodResult;
            }

            methodResult.Result = await _videoTimeCodeService.GetVideoTimeCodeDetailAsync(videoTimeCode, method.Result ?? new VideoTimeCodeResultModel(), request.IsShowSubStatus);
            return methodResult;
        }

        private async Task<VoidMethodResult> Validate(GetTimeCodeDetailQuery request, VideoResult videoResult, CancellationToken cancellationToken)
        {
            VoidMethodResult methodResult = new VoidMethodResult();

            var videoTimeCodes = await _videoTimeCodeRepository.ReadQueryable.Where(x => x.VideoId == videoResult.VideoId).OrderBy(x => x.DisplayTime).ToListAsync(cancellationToken);
            var videoTimeCodeRequest = videoTimeCodes.FirstOrDefault(x => x.Id == request.VideoTimeCodeId);
            if (videoTimeCodeRequest == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(videoTimeCodeRequest));
                return methodResult;
            }

            var (isErrorCode, displayTimeCodes) = await GetVideoTimeCodeAsync(videoTimeCodes, videoTimeCodeRequest, videoResult);
            if (isErrorCode)
            {
                methodResult.AddErrorBadRequest(new List<ErrorResult>
                {
                    new ErrorResult
                      {
                        ErrorCode = nameof(EnumVideoResultErrorCode.VideoTimeCodeNotCompleted),
                        Errors = new List<Error>
                        {
                            new Error
                            {
                                FieldName = nameof(videoTimeCodes),
                                ErrorValues = displayTimeCodes.Select(x=> new
                                {
                                    DisplayOrder = x.Item1,
                                    VideoTimeCodeId = x.Item2
                                }).Deserialize<IList<object>>()
                            }
                        }
                      }
                });
                return methodResult;
            }

            return methodResult;
        }

        private async Task<(bool, IList<(int, Guid)>)> GetVideoTimeCodeAsync(IList<VideoTimeCode>? videoTimeCodes, VideoTimeCode videoTimeCodeRequest, VideoResult videoResult)
        {
            var displayTimeCodes = new List<(int, Guid)>();
            if (videoTimeCodes != null)
            {
                var videoTimeCodePrevios = videoTimeCodes.Where(x => videoTimeCodes.IndexOf(x) < videoTimeCodes.IndexOf(videoTimeCodeRequest)).ToList();
                var videoTimeCodeResults = await _videoTimeCodeResultRepository.ReadQueryable.Where(x => x.VideoResultId == videoResult.Id).ToListAsync();
                if (videoTimeCodePrevios != null && videoTimeCodePrevios.Any())
                {
                    foreach (var videoTimeCode in videoTimeCodePrevios)
                    {
                        var videoTimeCodeResult = videoTimeCodeResults.FirstOrDefault(x => x.VideoTimeCodeId == videoTimeCode.Id);
                        if (videoTimeCodeResult == null || videoTimeCodeResult.Status != EnumResultStatus.Done)
                        {
                            displayTimeCodes.Add((videoTimeCodes.IndexOf(videoTimeCode) + 1, videoTimeCode.Id));
                        }
                    }
                }
            }

            return (displayTimeCodes.Any(), displayTimeCodes);
        }
    }
}
