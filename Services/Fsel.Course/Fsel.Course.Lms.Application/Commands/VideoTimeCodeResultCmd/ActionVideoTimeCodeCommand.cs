// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.VideoTimeCodeResultCmd
{
    using System.Threading;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
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
            var validateMethod = await Validate(videoResult, videoTimeCode, cancellationToken);
            if (!validateMethod.IsOK)
            {
                methodResult.AddErrorBadRequest(validateMethod.ErrorMessages);
                return methodResult;
            }

            var videoTimeCodeResult = await _videoTimeCodeResultRepository.ReadQueryable.FirstOrDefaultAsync(x => x.VideoResultId == request.VideoResultId && x.VideoTimeCodeId == request.VideoTimeCodeId, cancellationToken);
            if (videoTimeCodeResult == null)
            {
                videoResult.CurrentVideoTimeCodeId = request.VideoTimeCodeId;
                await _videoResultRepository.BulkUpdateList(new List<VideoResult> { videoResult }, bulk =>
                {
                    bulk.ColumnInputExpression = c => new { c.CurrentVideoTimeCodeId };
                });
            }
            methodResult.Result = _mapper.Map<VideoResultModel>(videoResult);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<VoidMethodResult> Validate(VideoResult videoResult, VideoTimeCode videoTimeCode, CancellationToken cancellationToken)
        {
            VoidMethodResult methodResult = new VoidMethodResult();

            var videoTimeCodes = await _videoTimeCodeRepository.ReadQueryable.Where(x => x.VideoId == videoResult.VideoId)
                                                               .OrderBy(x => x.DisplayTime)
                                                               .ToListAsync(cancellationToken);
            var videoTimeCodeRequest = videoTimeCodes.FirstOrDefault(x => x.Id == videoTimeCode.Id);
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
