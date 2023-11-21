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
    using Fsel.Course.Domain.Entities;
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
        private readonly AuthContext _authContext;
        private readonly AnswerTypeConverter _answerTypeConverter;
        private readonly QuestionConverter _questionConverter;
        private readonly IUserService _userService;
        private readonly IHomeWorkResultRepository _homeWorkResultRepository;

        public GetHomeWorkQueryHandler(IMapper mapper
            , IHomeWorkRepository homeWorkRepository
            , AuthContext authContext
            , AnswerTypeConverter answerTypeConverter
            , QuestionConverter questionConverter
            , IHomeWorkResultRepository homeWorkResult
            , IUserService userService)
        {
            _mapper = mapper;
            _homeWorkRepository = homeWorkRepository;
            _authContext = authContext;
            _answerTypeConverter = answerTypeConverter;
            _questionConverter = questionConverter;
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

            var homeWorkResult = await _homeWorkResultRepository.Queryable.FirstOrDefaultAsync(x => x.LessonResultId == request.LessonResultId && x.HomeWorkId == request.HomeWorkId && x.StudentId == studentId, cancellationToken);
            if (homeWorkResult == null || homeWorkResult.Status == EnumResultStatus.Unfinished)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(homeWorkResult));
                return methodResult;
            }
            var homeWork = await _homeWorkRepository.GetAsync(request.HomeWorkId, homeWorkResult);

            if (homeWork == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(homeWork));
                return methodResult;
            }
            methodResult.Result = GetHomeWork(homeWork, homeWorkResult);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private HomeWorkModel GetHomeWork(HomeWork homeWork, HomeWorkResult homeWorkResult)
        {
            var checkDone = homeWorkResult.Status == EnumResultStatus.Done;
            var homeWorkModel = _mapper.Map<HomeWorkModel>(homeWork);
            homeWorkModel.Questions = homeWork.HomeWorkQuestions.OrderBy(x => x!.CreatedDate).Select(n =>
            {
                var answer = n.HomeWorkAnswers.FirstOrDefault(n => n.HomeWorkResultId == homeWorkResult.Id);
                if (answer != null)
                {
                    answer.CorrectCount = checkDone ? answer.CorrectCount : default;
                    answer.Answer = _answerTypeConverter.AnswerTypeConverterObject(answer.Answer, n.Question!.QuestionType, !checkDone);
                }
                return _questionConverter.GetQuestion(n.Question ?? new Question(), answer, checkDone);
            }).ToList();
            homeWorkModel.HomeWorkResult = _mapper.Map<HomeWorkResultModel>(homeWorkResult);
            return homeWorkModel;
        }
    }
}
