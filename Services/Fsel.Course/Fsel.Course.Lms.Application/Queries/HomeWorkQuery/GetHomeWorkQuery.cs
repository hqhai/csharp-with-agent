// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.HomeWorkQuery
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetHomeWorkQuery : IRequest<MethodResult<HomeWorkModel>>
    {
        public Guid HomeWorkId { get; set; }
        public Guid LessonResultId { get; set; }
    }

    public class GetHomeWorkQueryHandler : IRequestHandler<GetHomeWorkQuery, MethodResult<HomeWorkModel>>
    {
        private readonly IMapper _mapper;
        private readonly IHomeWorkRepository _homeWorkRepository;
        private readonly QuestionTypeConverter _questionTypeConverter;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IHomeWorkResultRepository _homeWorkResultRepository;

        public GetHomeWorkQueryHandler(IMapper mapper
            , IHomeWorkRepository homeWorkRepository
            , QuestionTypeConverter questionTypeConverter
            , AuthContext authContext
            , IHomeWorkResultRepository homeWorkResult
            , IUserService userService)
        {
            _mapper = mapper;
            _homeWorkRepository = homeWorkRepository;
            _questionTypeConverter = questionTypeConverter;
            _authContext = authContext;
            _userService = userService;
            _homeWorkResultRepository = homeWorkResult;
        }

        public async Task<MethodResult<HomeWorkModel>> Handle(GetHomeWorkQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<HomeWorkModel> methodResult = new MethodResult<HomeWorkModel>();
            var studentsResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (studentsResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentsResult));
                return methodResult;
            }
            var studentId = studentsResult.Content?.Result?.Id;

            var homeWorkResult = await _homeWorkResultRepository.Queryable
                .FirstOrDefaultAsync(x => x.LessonResultId == request.LessonResultId && x.HomeWorkId == request.HomeWorkId && x.StudentId == studentId, cancellationToken);
            if (homeWorkResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(homeWorkResult));
                return methodResult;
            }
            var homeWork = await _homeWorkRepository.Queryable
                        .Include(x => x.HomeWorkQuestions)
                        .ThenInclude(x => x.HomeWorkAnswers.Where(n => n.HomeWorkResultId == homeWorkResult.Id))
                        .Include(x => x.HomeWorkQuestions)
                        .ThenInclude(x => x.Question)
                        .Include(x => x.HomeWorkResults.Where(x => x.Id == homeWorkResult.Id))
                        .Where(x => x.Id == request.HomeWorkId)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(cancellationToken);

            if (homeWork == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(homeWork));
                return methodResult;
            }

            var checkDone = homeWorkResult.Status == EnumResultStatus.Done;
            var homeWorkModel = new HomeWorkModel()
            {
                Id = homeWork.Id,
                Name = homeWork.Name,
                Code = homeWork.Code,
                MediaPost = homeWork.MediaPost,
                CourseLevel = homeWork.CourseLevel,
                CourseSkill = homeWork.CourseSkill,
                Questions = homeWork.HomeWorkQuestions.OrderBy(x => x!.CreatedDate).Select(n => new QuestionModel
                {
                    Id = n.Question!.Id,
                    CorrectTotal = n.Question!.CorrectTotal,
                    Ungraded = n.Question!.Ungraded,
                    Explanation = n.Question!.Explanation,
                    QuestionType = n.Question!.QuestionType,
                    Config = _questionTypeConverter.QuestionTypeConverterObject(n.Question.Config, n.Question.QuestionType, isDisableAnswers: !checkDone).Item1,
                    ResultAnswer = _mapper.Map<AnswerModel>(n.HomeWorkAnswers.FirstOrDefault(n => n.HomeWorkResultId == homeWorkResult.Id))
                }).ToList(),
                HomeWorkResult = homeWork.HomeWorkResults.Where(x => x.Id == homeWorkResult.Id).Select(x => new HomeWorkResultModel
                {
                    Id = x.Id,
                    HomeWorkId = x.HomeWorkId,
                    LessonResultId = x.LessonResultId,
                    CorrectCount = x.CorrectCount,
                    CorrectTotal = x.CorrectTotal,
                    Percent = x.Percent,
                    Status = x.Status,
                    StudentId = x.StudentId,
                }).FirstOrDefault()
            };

            methodResult.Result = homeWorkModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
