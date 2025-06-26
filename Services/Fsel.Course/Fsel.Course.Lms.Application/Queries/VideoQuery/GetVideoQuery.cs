// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.VideoQuery
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetVideoQuery : IRequest<MethodResult<VideoModel>>
    {
        public Guid VideoId { get; set; }
        public Guid? LessonResultId { get; set; }
    }

    public class GetVideoStandaloneQueryHandler : IRequestHandler<GetVideoQuery, MethodResult<VideoModel>>
    {
        private readonly IVideoRepository _videoRepository;
        private readonly IVideoResultRepository _videoResultRepository;
        private readonly AuthContext _authContext;
        private readonly IMapper _mapper;
        private readonly VideoConverter _videoConverter;
        private readonly IUserService _userService;

        public GetVideoStandaloneQueryHandler(IVideoRepository videoRepository,
            IVideoResultRepository videoResultRepository,
            AuthContext authContext,
            IMapper mapper,
            VideoConverter videoConverter,
            IUserService userService)
        {
            _videoRepository = videoRepository;
            _videoResultRepository = videoResultRepository;
            _authContext = authContext;
            _mapper = mapper;
            _videoConverter = videoConverter;
            _userService = userService;
        }

        public async Task<MethodResult<VideoModel>> Handle(GetVideoQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<VideoModel> methodResult = new MethodResult<VideoModel>();

            var studentsResult = await _userService.GetStudentByUserIdWithCacheAsync(_authContext.CurrentUserId);
            if (studentsResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentsResult));
                return methodResult;
            }
            var studentId = studentsResult.Content!.Result!.Id;

            var videoResult = await _videoResultRepository.Queryable.Where(x => !request.LessonResultId.HasValue || x.LessonResultId == request.LessonResultId)
                        .FirstOrDefaultAsync(x => x.VideoId == request.VideoId && x.StudentId == studentId, cancellationToken);
            if (videoResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(videoResult));
                return methodResult;
            }

            var video = await _videoRepository.Queryable
                                .Include(x => x.LessonVideos.Where(y => !y.IsDeleted))
                                .Include(i => i.VideoTimeCodes.Where(x => !x.IsDeleted))
                                    .ThenInclude(x => x.TimeCodeExercises.Where(x => !x.IsDeleted && x.Exercise != null))
                                    .ThenInclude(x => x.Exercise)
                                    .ThenInclude(x => x!.ExerciseQuestions.Where(x => !x.IsDeleted))
                                    .ThenInclude(x => x.Question)
                                    .ThenInclude(x => x!.VideoTimeCodeAnswers.Where(x => x.VideoResultId == videoResult.Id))
                                .Include(i => i.VideoResults.Where(x => !x.IsDeleted))
                                .Include(i => i.VideoTimeCodes.Where(x => !x.IsDeleted))
                                .ThenInclude(x => x!.VideoTimeCodeResults.Where(x => x.VideoResultId == videoResult.Id))
                                .Include(i => i.VideoSubFilePaths.Where(x => !x.IsDeleted))
                                .Where(x => x.Id == request.VideoId)
                                .AsNoTracking()
                                .FirstOrDefaultAsync(cancellationToken: cancellationToken);

            if (video == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(video));
                return methodResult;
            }
            var videoModel = _mapper.Map<VideoModel>(video);
            videoModel.VideoTimeCodes = await _videoConverter.GetTimeCodes(video, videoResult, true);
            videoModel.VideoResult = _mapper.Map<VideoResultModel>(videoResult);
            methodResult.Result = videoModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
