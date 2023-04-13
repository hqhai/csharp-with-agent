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

            var studentsResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId.ToString());
            if (studentsResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseClassStudentErrorCode.UserIdNotExist));
                return methodResult;
            }
            var studentId = studentsResult.Content!.Result!.Id;
            var homeWorkResult = await _homeWorkResultRepository.Queryable
                .Include(x => x.HomeWorkAnswers.Where(y => !y.IsDeleted))
                .Include(x => x.HomeWork)
                .ThenInclude(x => x!.HomeWorkQuestions.Where(y => !y.IsDeleted && y.Question != null))
                .ThenInclude(x => x.Question)
                .Where(x => !request.LessonResultId.HasValue || x.LessonResultId == request.LessonResultId)
                .FirstOrDefaultAsync(x => x.HomeWorkId == request.HomeWorkId && x.StudentId == studentId, cancellationToken);

            if (homeWorkResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumHomeWorkErrorCode.HomeWorkNotExist), nameof(request.HomeWorkId), request?.HomeWorkId);
                return methodResult;
            }
            var checkDone = homeWorkResult != null && homeWorkResult.Status == EnumResultStatus.Done;

            var homeWorkModel = new HomeWorkModel()
            {
                Id = homeWorkResult.HomeWork.Id,
                Name = homeWorkResult.HomeWork.Name,
                Code = homeWorkResult.HomeWork.Code,
                MediaPost = homeWorkResult.HomeWork.MediaPost,
                Questions = homeWorkResult.HomeWork.HomeWorkQuestions.Select(x => x.Question).Select(n => new QuestionModel
                {
                    Id = n!.Id,
                    CorrectTotal = n!.CorrectTotal,
                    Ungraded = n!.Ungraded,
                    Explanation = n!.Explanation,
                    QuestionType = n!.QuestionType,
                    Config = _questionTypeConverter.QuestionTypeConverterObject(n.Config, n.QuestionType, isDisableAnswers: !checkDone).Item1,
                }).ToList(),
                HomeWorkAnswers = _mapper.Map<List<HomeWorkAnswerModel>>(homeWorkResult.HomeWorkAnswers.ToList()),
            };

            methodResult.Result = homeWorkModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
