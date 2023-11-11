// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.VideoQuery
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
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

        public GetTimeCodeDetailQueryHandler(IVideoTimeCodeRepository videoTimeCodeRepository,
            IVideoResultRepository videoResultRepository,
            AuthContext authContext,
            IMediator mediator,
            VideoConverter videoConverter,
            IUserService userService)
        {
            _videoTimeCodeRepository = videoTimeCodeRepository;
            _videoResultRepository = videoResultRepository;
            _authContext = authContext;
            _mediator = mediator;
            _videoConverter = videoConverter;
            _userService = userService;
        }

        public async Task<MethodResult<VideoTimeCodeModel>> Handle(GetTimeCodeDetailQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<VideoTimeCodeModel> methodResult = new MethodResult<VideoTimeCodeModel>();

            var studentsResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
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
            var videoTimeCode = await _videoTimeCodeRepository.Queryable
                                    .Include(x => x.VideoTimeCodeAnswers.Where(x => x.VideoResultId == videoResult.Id))
                                    .Include(x => x.TimeCodeExercises.Where(x => !x.IsDeleted && x.Exercise != null))
                                    .ThenInclude(x => x.Exercise)
                                    .ThenInclude(x => x!.ExerciseQuestions.Where(x => !x.IsDeleted))
                                    .ThenInclude(x => x.Question)
                                    .ThenInclude(x => x!.VideoTimeCodeAnswers.Where(x => videoResult != null && x.VideoResultId == videoResult.Id && x.VideoTimeCodeId == request.VideoTimeCodeId))
                                .Where(x => x.Id == request.VideoTimeCodeId && x.VideoId == request.VideoId)
                                .AsNoTracking()
                                .FirstOrDefaultAsync(cancellationToken: cancellationToken);

            if (videoTimeCode == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(videoTimeCode));
                return methodResult;
            }
            var method = await _mediator.Send(new CreateVideoTimeCodeResultCommand { VideoResultId = videoResult.Id, StudentId = studentId, VideoTimeCodeId = request.VideoTimeCodeId }, cancellationToken);
            if (!method.IsOK)
            {
                methodResult.AddErrorBadRequest(method.ErrorMessages);
                return methodResult;
            }
            var videoTimeCodeModel = _videoConverter.GetVideoTimeCode(videoTimeCode, method.Result);
            methodResult.Result = videoTimeCodeModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
