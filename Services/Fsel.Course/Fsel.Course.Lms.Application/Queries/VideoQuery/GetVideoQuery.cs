// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.VideoQuery
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
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
        private readonly QuestionTypeConverter _questionTypeConverter;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public GetVideoStandaloneQueryHandler(IVideoRepository videoRepository,
            QuestionTypeConverter questionTypeConverter,
            IVideoResultRepository videoResultRepository,
            AuthContext authContext,
            IUserService userService,
            IMapper mapper)
        {
            _videoRepository = videoRepository;
            _videoResultRepository = videoResultRepository;
            _questionTypeConverter = questionTypeConverter;
            _authContext = authContext;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<MethodResult<VideoModel>> Handle(GetVideoQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<VideoModel> methodResult = new MethodResult<VideoModel>();

            var studentsResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (studentsResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumVideoErrorCode.UserNotExist));
                return methodResult;
            }
            var studentId = studentsResult.Content!.Result!.Id;

            var videoResult = await _videoResultRepository.Queryable.Where(x => !request.LessonResultId.HasValue || x.LessonResultId == request.LessonResultId)
                .FirstOrDefaultAsync(x => x.VideoId == request.VideoId && x.StudentId == studentId, cancellationToken);

            var video = await _videoRepository.Queryable
                                .Include(x => x.LessonVideos.Where(y => !y.IsDeleted))
                                .Include(i => i.VideoTimeCodes.Where(x => !x.IsDeleted))
                                    .ThenInclude(x => x.TimeCodeExercises.Where(x => !x.IsDeleted && x.Exercise != null))
                                    .ThenInclude(x => x.Exercise)
                                    .ThenInclude(x => x!.ExerciseQuestions.Where(x => !x.IsDeleted))
                                    .ThenInclude(x => x.Question)
                                    .ThenInclude(x => x!.VideoTimeCodeAnswers!.Where(x => videoResult != null && x.VideoResultId == videoResult.Id))
                                .Include(i => i.VideoResults.Where(x => !x.IsDeleted))
                                .Where(x => x.Id == request.VideoId)
                                .AsNoTracking()
                                .FirstOrDefaultAsync(cancellationToken: cancellationToken);

            if (video == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumVideoErrorCode.VideoNotExist), nameof(request.VideoId), request.VideoId);
                return methodResult;
            }

            var checkDone = videoResult != null && videoResult.Status == EnumResultStatus.Done;
            var videoModel = new VideoModel
            {
                Id = video.Id,
                Name = video.Name,
                VideoFilePath = video.VideoFilePath,
                IsActive = video.LessonVideos.Any(),
                TeacherId = video.TeacherId,
                CourseLevel = video.CourseLevel,
                VideoTimeCodes = video.VideoTimeCodes.OrderBy(x => x!.DisplayTime).Select(x => new VideoTimeCodeModel
                {
                    Id = x.Id,
                    DisplayTime = x.DisplayTime,
                    ExecutionTime = x.ExecutionTime,
                    TimeCodeType = x.TimeCodeType,
                    VideoId = x.VideoId,
                    Exercises = x.TimeCodeExercises.Where(n => n.Exercise != null).OrderBy(x => x!.CreatedDate).Select(n => n.Exercise).Select(n => new ExerciseModel
                    {
                        Id = n!.Id,
                        MediaPost = n.MediaPost,
                        CourseSkill = n.CourseSkill,
                        Questions = n.ExerciseQuestions.Where(m => m.Question != null).OrderBy(x => x!.CreatedDate).Select(m => m.Question).Select(m => new QuestionModel()
                        {
                            Id = m!.Id,
                            QuestionType = m.QuestionType,
                            CorrectTotal = m.CorrectTotal,
                            Explanation = m.Explanation,
                            Ungraded = m.Ungraded,
                            Config = _questionTypeConverter.QuestionTypeConverterObject(m.Config, m.QuestionType, isDisableAnswers: !checkDone).Item1,
                            ResultAnswer = _mapper.Map<VideoTimeCodeAnswerModel>(m.VideoTimeCodeAnswers!.FirstOrDefault())
                        }).ToList()
                    }).ToList(),
                }).ToList(),
                VideoResult = video.VideoResults.Where(x => x.StudentId == studentId).Select(x => new VideoResultModel
                {
                    Id = x.Id,
                    CorrectCount = x.CorrectCount,
                    CorrectTotal = x.CorrectTotal,
                    Feedback = x.Feedback,
                    NumberOfStars = x.NumberOfStars,
                    Percent = x.Percent,
                    Status = x.Status,
                    LessonResultId = x.LessonResultId,
                    StudentId = x.StudentId,
                    VideoId = x.VideoId,
                }).FirstOrDefault(),
            };
            methodResult.Result = videoModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
