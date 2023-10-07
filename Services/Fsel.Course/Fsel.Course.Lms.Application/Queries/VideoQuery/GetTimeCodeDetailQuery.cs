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
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
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
        private readonly QuestionTypeConverter _questionTypeConverter;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public GetTimeCodeDetailQueryHandler(IVideoTimeCodeRepository videoTimeCodeRepository,
            QuestionTypeConverter questionTypeConverter,
            IVideoResultRepository videoResultRepository,
            AuthContext authContext,
            IUserService userService,
            IMapper mapper)
        {
            _videoTimeCodeRepository = videoTimeCodeRepository;
            _videoResultRepository = videoResultRepository;
            _questionTypeConverter = questionTypeConverter;
            _authContext = authContext;
            _userService = userService;
            _mapper = mapper;
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
            var studentId = studentsResult.Content?.Result?.Id;

            var videoResult = await _videoResultRepository.Queryable.Include(x => x.VideoTimeCodeResults.Where(x => x.VideoTimeCodeId == request.VideoTimeCodeId))
                        .ThenInclude(x => x.VideoTimeCodeAnswers)
                        .Where(x => !request.LessonResultId.HasValue || x.LessonResultId == request.LessonResultId)
                        .FirstOrDefaultAsync(x => x.VideoId == request.VideoId && x.StudentId == studentId, cancellationToken);
            if (videoResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(videoResult));
                return methodResult;
            }

            var videoTimeCodeResult = videoResult.VideoTimeCodeResults.FirstOrDefault();
            var videoTimeCodeResultId = videoTimeCodeResult?.Id ?? default;
            var videoTimeCode = await _videoTimeCodeRepository.Queryable
                                    .Include(x => x.TimeCodeExercises.Where(x => !x.IsDeleted && x.Exercise != null))
                                    .ThenInclude(x => x.Exercise)
                                    .ThenInclude(x => x!.ExerciseQuestions.Where(x => !x.IsDeleted))
                                    .ThenInclude(x => x.Question)
                                    .ThenInclude(x => x!.VideoTimeCodeAnswers.Where(x => videoResult != null && x.VideoTimeCodeResultId == videoTimeCodeResultId))
                                .Where(x => x.Id == request.VideoTimeCodeId && x.VideoId == request.VideoId)
                                .AsNoTracking()
                                .FirstOrDefaultAsync(cancellationToken: cancellationToken);

            if (videoTimeCode == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(videoTimeCode));
                return methodResult;
            }

            var videoTimeCodeModel = new VideoTimeCodeModel
            {
                Id = videoTimeCode.Id,
                TotalCount = videoTimeCode.TimeCodeExercises.Where(x => !x.IsDeleted && x.Exercise != null).Select(x => x.Exercise).SelectMany(x => x!.ExerciseQuestions.Where(x => !x.IsDeleted && x.Question != null)).Select(m => m.Question).Count(),
                DisplayTime = videoTimeCode.DisplayTime,
                ExecutionTime = videoTimeCode.ExecutionTime,
                TimeCodeType = videoTimeCode.TimeCodeType,
                VideoId = videoTimeCode.VideoId,
                Ungraded = videoTimeCode.TimeCodeExercises.Select(x => x.Exercise).SelectMany(x => x!.ExerciseQuestions).Select(x => x.Question).FirstOrDefault()?.Ungraded ?? default,
                CorrectCount = videoTimeCodeResult.VideoTimeCodeAnswers.Any() ? videoTimeCode.VideoTimeCodeAnswers.Sum(x => x.CorrectCount) : 0,
                CorrectTotal = videoTimeCode.TimeCodeExercises.Select(x => x.Exercise).SelectMany(x => x!.ExerciseQuestions).Select(x => x.Question).Sum(x => x!.CorrectTotal),
                Status = (videoTimeCode.VideoTimeCodeAnswers.Count > 0 && videoTimeCode.VideoTimeCodeAnswers.All(y => videoResult != null && y.VideoResultId == videoResult.Id && y.Status == EnumTimeCodeStatus.Done)) ? EnumTimeCodeStatus.Done : EnumTimeCodeStatus.Process,
                Exercises = videoTimeCode.TimeCodeExercises.OrderBy(x => x!.CreatedDate).Select(n => n.Exercise).Select(n => new ExerciseModel
                {
                    Id = n!.Id,
                    MediaPost = n.MediaPost,
                    Name = n.Name,
                    CourseSkill = n.CourseSkill,
                    Questions = n.ExerciseQuestions.OrderBy(x => x!.CreatedDate).Select(m => m.Question).Select(m => new QuestionModel()
                    {
                        Id = m!.Id,
                        QuestionType = m.QuestionType,
                        CorrectTotal = m.CorrectTotal,
                        Explanation = m.Explanation,
                        Ungraded = m.Ungraded,
                        Config = _questionTypeConverter.QuestionTypeConverterObject(m.Config, m.QuestionType, isDisableAnswers: !(m.VideoTimeCodeAnswers.FirstOrDefault()?.Status == EnumTimeCodeStatus.Done)).Item1,
                        ResultAnswer = _mapper.Map<AnswerModel>(m.VideoTimeCodeAnswers!.FirstOrDefault())
                    }).ToList()
                }).ToList(),
            };
            methodResult.Result = videoTimeCodeModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private VideoTimeCodeModel GetTimeCode(VideoTimeCode videoTimeCode, VideoTimeCodeResult videoTimeCodeResult)
        {
            return new VideoTimeCodeModel
            {
                Id = videoTimeCode.Id,
                TotalCount = videoTimeCode.TimeCodeExercises.Where(x => !x.IsDeleted && x.Exercise != null).Select(x => x.Exercise).SelectMany(x => x!.ExerciseQuestions.Where(x => !x.IsDeleted && x.Question != null)).Select(m => m.Question).Count(),
                DisplayTime = videoTimeCode.DisplayTime,
                ExecutionTime = videoTimeCode.ExecutionTime,
                TimeCodeType = videoTimeCode.TimeCodeType,
                VideoId = videoTimeCode.VideoId,
                Ungraded = videoTimeCode.TimeCodeExercises.Select(x => x.Exercise).SelectMany(x => x!.ExerciseQuestions).Select(x => x.Question).FirstOrDefault()?.Ungraded ?? default,
                CorrectCount = videoTimeCodeResult.VideoTimeCodeAnswers.Any() ? videoTimeCode.VideoTimeCodeAnswers.Sum(x => x.CorrectCount) : 0,
                CorrectTotal = videoTimeCode.TimeCodeExercises.Select(x => x.Exercise).SelectMany(x => x!.ExerciseQuestions).Select(x => x.Question).Sum(x => x!.CorrectTotal),
                Status = (videoTimeCodeResult.VideoTimeCodeAnswers.Count > 0 && videoTimeCode.VideoTimeCodeAnswers.All(y => videoResult != null && y.VideoResultId == videoResult.Id && y.Status == EnumTimeCodeStatus.Done)) ? EnumTimeCodeStatus.Done : EnumTimeCodeStatus.Process,
                Exercises = videoTimeCode.TimeCodeExercises.OrderBy(x => x!.CreatedDate).Select(n => n.Exercise).Select(n => new ExerciseModel
                {
                    Id = n!.Id,
                    MediaPost = n.MediaPost,
                    Name = n.Name,
                    CourseSkill = n.CourseSkill,
                    Questions = n.ExerciseQuestions.OrderBy(x => x!.CreatedDate).Select(m => m.Question).Select(m => new QuestionModel()
                    {
                        Id = m!.Id,
                        QuestionType = m.QuestionType,
                        CorrectTotal = m.CorrectTotal,
                        Explanation = m.Explanation,
                        Ungraded = m.Ungraded,
                        Config = _questionTypeConverter.QuestionTypeConverterObject(m.Config, m.QuestionType, isDisableAnswers: !(m.VideoTimeCodeAnswers.FirstOrDefault()?.Status == EnumTimeCodeStatus.Done)).Item1,
                        ResultAnswer = _mapper.Map<AnswerModel>(m.VideoTimeCodeAnswers!.FirstOrDefault())
                    }).ToList()
                }).ToList(),
            };
        }
    }
}
