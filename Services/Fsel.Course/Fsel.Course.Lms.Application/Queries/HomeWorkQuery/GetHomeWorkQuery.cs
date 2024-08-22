// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.HomeWorkQuery
{
    using System;
    using System.Linq;
    using System.Text.Json.Serialization;
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
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetHomeWorkQuery : IRequest<MethodResult<HomeWorkModel>>
    {
        public Guid HomeWorkId { get; set; }
        public Guid LessonResultId { get; set; }

        [JsonIgnore]
        public bool IsShowSubStatus { get; set; }
    }

    public class GetHomeWorkQueryHandler : IRequestHandler<GetHomeWorkQuery, MethodResult<HomeWorkModel>>
    {
        private readonly IMapper _mapper;
        private readonly IHomeWorkRepository _homeWorkRepository;
        private readonly AuthContext _authContext;
        private readonly QuestionTypeConverter _questionTypeConverter;
        private readonly AnswerTypeConverter _answerTypeConverter;
        private readonly IUserService _userService;
        private readonly IHomeWorkResultRepository _homeWorkResultRepository;

        public GetHomeWorkQueryHandler(IMapper mapper
            , IHomeWorkRepository homeWorkRepository
            , AuthContext authContext
            , QuestionTypeConverter questionTypeConverter
            , AnswerTypeConverter answerTypeConverter
            , IHomeWorkResultRepository homeWorkResult
            , IUserService userService)
        {
            _mapper = mapper;
            _homeWorkRepository = homeWorkRepository;
            _authContext = authContext;
            _questionTypeConverter = questionTypeConverter;
            _answerTypeConverter = answerTypeConverter;
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
            if (homeWorkResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(homeWorkResult));
                return methodResult;
            }
            else if (homeWorkResult.Status == EnumResultStatus.Unfinished)
            {
                methodResult.AddErrorBadRequest(nameof(EnumResultErrorCode.ResultStatusUnfinished), nameof(homeWorkResult));
                return methodResult;
            }
            var homeWork = await _homeWorkRepository.GetAsync(homeWorkResult);

            if (homeWork == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(homeWork));
                return methodResult;
            }
            methodResult.Result = GetHomeWork(homeWork, homeWorkResult, request.IsShowSubStatus);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private HomeWorkModel GetHomeWork(HomeWork homeWork, HomeWorkResult homeWorkResult, bool isShowSubStatus)
        {
            var homeWorkModel = _mapper.Map<HomeWorkModel>(homeWork);
            homeWorkModel.Questions = homeWork.HomeWorkQuestions.OrderBy(x => x!.CreatedDate).Select(n =>
            {
                var answer = n.HomeWorkAnswers.FirstOrDefault(n => n.HomeWorkResultId == homeWorkResult.Id);
                return GetQuestion(n.Question, answer, homeWorkResult, isShowSubStatus);
            }).ToList();
            homeWorkModel.HomeWorkResult = _mapper.Map<HomeWorkResultModel>(homeWorkResult);
            return homeWorkModel;
        }

        private QuestionModel GetQuestion(Question? question, HomeWorkAnswer? homeWorkAnswer, HomeWorkResult homeWorkResult, bool isShowSubStatus)
        {
            ArgumentNullException.ThrowIfNull(question);
            var isCheck = homeWorkAnswer?.Status == EnumAnswerStatus.Done;
            var questionModel = _mapper.Map<QuestionModel>(question);
            questionModel.CorrectStatus = GetCorrectStatus(homeWorkAnswer);
            questionModel.Config = _questionTypeConverter.QuestionTypeConverterObject(question.Config, question.QuestionType, isDisableAnswers: !(isCheck)).Item1;
            if (homeWorkAnswer != null)
            {
                homeWorkAnswer.CorrectCount = isCheck ? homeWorkAnswer.CorrectCount : default;
                homeWorkAnswer.IsCorrect = isCheck ? homeWorkAnswer.IsCorrect : default;
                homeWorkAnswer.Answer = _answerTypeConverter.AnswerTypeConverterObject(homeWorkAnswer.Answer, question.QuestionType, isShowSubStatus, homeWorkResult.Status, homeWorkResult.SubmissionCount == EnumSubmissionCount.SecondSubmit);
                questionModel.ResultAnswer = _mapper.Map<AnswerModel>(homeWorkAnswer);
            }
            return questionModel;
        }

        private static EnumCorrectStatus? GetCorrectStatus(HomeWorkAnswer? homeWorkAnswer)
        {
            if (homeWorkAnswer == null)
            {
                return default;
            }
            return homeWorkAnswer.Status == EnumAnswerStatus.Done ? (homeWorkAnswer.IsCorrect.HasValue && homeWorkAnswer.IsCorrect.Value ? EnumCorrectStatus.Correct : EnumCorrectStatus.Fail)
                : EnumCorrectStatus.Process;
        }
    }
}
