// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ExtraPracticeQuery
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
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetTimeCodeDetailByExtraPracticeQuery : IRequest<MethodResult<VideoTimeCodeModel>>
    {
        public Guid ExtraPracticeId { get; set; }
        public Guid VideoId { get; set; }
        public Guid VideoTimeCodeId { get; set; }
    }

    public class GetTimeCodeDetailByExtraPracticeQueryHandler : IRequestHandler<GetTimeCodeDetailByExtraPracticeQuery, MethodResult<VideoTimeCodeModel>>
    {
        private readonly IVideoTimeCodeRepository _videoTimeCodeRepository;
        private readonly IExtraPracticeResultRepository _extraPracticeResultRepository;
        private readonly QuestionTypeConverter _questionTypeConverter;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public GetTimeCodeDetailByExtraPracticeQueryHandler(IVideoTimeCodeRepository videoTimeCodeRepository,
            IExtraPracticeResultRepository extraPracticeResultRepository,
            QuestionTypeConverter questionTypeConverter,
            AuthContext authContext,
            IUserService userService,
            IMapper mapper)
        {
            _videoTimeCodeRepository = videoTimeCodeRepository;
            _extraPracticeResultRepository = extraPracticeResultRepository;
            _questionTypeConverter = questionTypeConverter;
            _authContext = authContext;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<MethodResult<VideoTimeCodeModel>> Handle(GetTimeCodeDetailByExtraPracticeQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<VideoTimeCodeModel> methodResult = new MethodResult<VideoTimeCodeModel>();

            var studentsResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (studentsResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }
            var studentId = studentsResult.Content?.Result?.Id;

            var extraPracticeResult = await _extraPracticeResultRepository.Queryable.FirstOrDefaultAsync(x => x.ExtraPracticeId == request.ExtraPracticeId && x.StudentId == studentId, cancellationToken);

            var videoTimeCode = await _videoTimeCodeRepository.Queryable
                                    .Include(x => x.TimeCodeExercises.Where(x => !x.IsDeleted && x.Exercise != null))
                                    .ThenInclude(x => x.Exercise)
                                    .ThenInclude(x => x!.ExerciseQuestions.Where(x => !x.IsDeleted))
                                    .ThenInclude(x => x.Question)
                                    .ThenInclude(x => x!.ExtraPracticeAnswers!.Where(x => extraPracticeResult != null && x.ExtraPracticeResultId == extraPracticeResult.Id))
                                .Where(x => x.Id == request.VideoTimeCodeId && x.VideoId == request.VideoId)
                                .AsNoTracking()
                                .FirstOrDefaultAsync(cancellationToken: cancellationToken);

            if (videoTimeCode == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.VideoId), request.VideoId);
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
                CorrectCount = videoTimeCode.VideoTimeCodeAnswers.Count > 0 ? videoTimeCode.VideoTimeCodeAnswers.Sum(x => x.CorrectCount) : 0,
                CorrectTotal = videoTimeCode.TimeCodeExercises.Select(x => x.Exercise).SelectMany(x => x!.ExerciseQuestions).Select(x => x.Question).Sum(x => x!.CorrectTotal),
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
                        Config = _questionTypeConverter.QuestionTypeConverterObject(m.Config, m.QuestionType, isDisableAnswers: !(m.VideoTimeCodeAnswers.FirstOrDefault()?.Status == EnumCurrentStatus.Done)).Item1,
                        ResultAnswer = _mapper.Map<AnswerModel>(m.ExtraPracticeAnswers!.FirstOrDefault())
                    }).ToList()
                }).ToList(),
            };
            methodResult.Result = videoTimeCodeModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
