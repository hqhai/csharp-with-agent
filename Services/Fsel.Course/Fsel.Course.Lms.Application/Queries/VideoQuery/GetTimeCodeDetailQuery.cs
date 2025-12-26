// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.VideoQuery
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Commands.VideoTimeCodeAnswerCmd;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetTimeCodeDetailQuery : IRequest<MethodResult<VideoTimeCodeModel>>
    {
        public Guid VideoId { get; set; }
        public Guid VideoTimeCodeId { get; set; }
        public bool IsShowSubStatus { get; set; }
        public bool IsCreateAnswer { get; set; }
        public Guid? LessonResultId { get; set; }
    }

    public class GetTimeCodeDetailQueryHandler : IRequestHandler<GetTimeCodeDetailQuery, MethodResult<VideoTimeCodeModel>>
    {
        private readonly IVideoTimeCodeRepository _videoTimeCodeRepository;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly AuthContext _authContext;
        private readonly IMediator _mediator;
        private readonly VideoConverter _videoConverter;
        private readonly IUserService _userService;
        private readonly IVideoTimeCodeResultRepository _videoTimeCodeResultRepository;

        public GetTimeCodeDetailQueryHandler(IVideoTimeCodeRepository videoTimeCodeRepository,
            IVideoResultRepository videoResultRepository,
            AuthContext authContext,
            IMediator mediator,
            VideoConverter videoConverter,
            IUserService userService,
            IVideoTimeCodeResultRepository videoTimeCodeResultRepository)
        {
            _videoTimeCodeRepository = videoTimeCodeRepository;
            _videoResultRepository = videoResultRepository;
            _authContext = authContext;
            _mediator = mediator;
            _videoConverter = videoConverter;
            _userService = userService;
            _videoTimeCodeResultRepository = videoTimeCodeResultRepository;
        }

        public async Task<MethodResult<VideoTimeCodeModel>> Handle(GetTimeCodeDetailQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<VideoTimeCodeModel> methodResult = new MethodResult<VideoTimeCodeModel>();

            var studentsResult = await _userService.GetStudentByUserIdWithCacheAsync(_authContext.CurrentUserId);
            if (studentsResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentsResult));
                return methodResult;
            }
            var studentId = studentsResult.Content?.Result?.Id ?? default;

            var videoResult = await _videoResultRepository.Queryable
                        .Where(x => !request.LessonResultId.HasValue || x.LessonResultId == request.LessonResultId)
                        .FirstOrDefaultAsync(x => x.VideoId == request.VideoId && x.StudentId == studentId, cancellationToken);
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

            methodResult.Result = await _videoConverter.GetVideoTimeCodeDetailAsync(videoTimeCode, method.Result ?? new VideoTimeCodeResultModel(), request.IsShowSubStatus);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<VoidMethodResult> Validate(GetTimeCodeDetailQuery request, VideoResult videoResult, CancellationToken cancellationToken)
        {
            VoidMethodResult methodResult = new VoidMethodResult();

            var videoTimeCodes = await _videoTimeCodeRepository.Queryable.Where(x => x.VideoId == videoResult.VideoId).OrderBy(x => x.DisplayTime).ToListAsync(cancellationToken);
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
                var videoTimeCodeResults = await _videoTimeCodeResultRepository.Queryable.Where(x => x.VideoResultId == videoResult.Id && x.CreatedDate >= videoResult.CreatedDate)
                    .Where(x => !(videoResult.Status == EnumResultStatus.Done) || x.CreatedDate <= videoResult.UpdatedDate)
                    .ToListAsync();
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
