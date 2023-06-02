// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.HomeWorkQuery
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

    public class GetHomeWorkQuery : IRequest<MethodResult<HomeWorkModel>>
    {
        public Guid HomeWorkId { get; set; }
        public Guid? LessonResultId { get; set; }
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
            MethodResult<HomeWorkModel> methodResult = new MethodResult<HomeWorkModel>();

            var studentsResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (studentsResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumHomeWorkErrorCode.UserNotExist));
                return methodResult;
            }
            var studentId = studentsResult.Content!.Result!.Id;

            var homeWorkResult = await _homeWorkResultRepository.Queryable
                .FirstOrDefaultAsync(x => x.LessonResultId == request.LessonResultId && x.HomeWorkId == request.HomeWorkId && x.StudentId == studentId, cancellationToken);

            var homeWork = await _homeWorkRepository.Queryable
                        .Include(x => x.LessonHomeWorks.Where(n => !n.IsDeleted))
                        .Include(x => x.HomeWorkQuestions.Where(n => !n.IsDeleted))
                        .ThenInclude(x => x.HomeWorkAnswers.Where(n => !n.IsDeleted && homeWorkResult != null && n.HomeWorkResultId == homeWorkResult.Id))
                        .Include(x => x.HomeWorkQuestions.Where(n => !n.IsDeleted))
                        .ThenInclude(x => x.Question)
                        .Include(x => x.HomeWorkResults.Where(n => !n.IsDeleted))
                        .Where(x => x.Id == request.HomeWorkId)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(cancellationToken: cancellationToken);

            if (homeWork == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumHomeWorkErrorCode.HomeWorkNotExist), nameof(request.HomeWorkId), request?.HomeWorkId);
                return methodResult;
            }

            var checkDone = homeWorkResult != null && homeWorkResult.Status == EnumResultStatus.Done;
            var homeWorkModel = new HomeWorkModel()
            {
                Id = homeWork.Id,
                Name = homeWork.Name,
                Code = homeWork.Code,
                MediaPost = homeWork.MediaPost,
                CourseLevel = homeWork.CourseLevel,
                CourseSkill = homeWork.CourseSkill,
                IsActive = homeWork.LessonHomeWorks.Any(),
                Questions = homeWork.HomeWorkQuestions.Where(x => x.Question != null).OrderBy(x => x!.CreatedDate).Select(n => new QuestionModel
                {
                    Id = n.Question!.Id,
                    CorrectTotal = n.Question!.CorrectTotal,
                    Ungraded = n.Question!.Ungraded,
                    Explanation = n.Question!.Explanation,
                    QuestionType = n.Question!.QuestionType,
                    Config = _questionTypeConverter.QuestionTypeConverterObject(n.Question.Config, n.Question.QuestionType, isDisableAnswers: !checkDone).Item1,
                    ResultAnswer = _mapper.Map<HomeWorkAnswerModel>(n.HomeWorkAnswers.FirstOrDefault())
                }).ToList(),
                HomeWorkResult = homeWork.HomeWorkResults.Where(x => x.StudentId == studentId).Select(x => new HomeWorkResultModel
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
