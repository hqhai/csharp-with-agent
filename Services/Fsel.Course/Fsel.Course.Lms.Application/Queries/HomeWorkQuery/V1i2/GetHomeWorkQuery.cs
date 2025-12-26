// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.HomeWorkQuery.V1i2
{
    using System;
    using System.Collections.Generic;
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
        public Guid LessonModuleId { get; set; }
        public Guid HomeWorkResultId { get; set; }

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
        private readonly IQuestionShuffleRepository _questionShuffleRepository;
        private readonly IQuestionExplanationErrorRepository _questionExplanationErrorRepository;
        private readonly IHomeWorkResultRepository _homeWorkResultRepository;

        public GetHomeWorkQueryHandler(IMapper mapper
            , IHomeWorkRepository homeWorkRepository
            , AuthContext authContext
            , QuestionTypeConverter questionTypeConverter
            , AnswerTypeConverter answerTypeConverter
            , IHomeWorkResultRepository homeWorkResult
            , IUserService userService
            , IQuestionShuffleRepository questionShuffleRepository
            , IQuestionExplanationErrorRepository questionExplanationErrorRepository)
        {
            _mapper = mapper;
            _homeWorkRepository = homeWorkRepository;
            _authContext = authContext;
            _questionTypeConverter = questionTypeConverter;
            _answerTypeConverter = answerTypeConverter;
            _userService = userService;
            _questionShuffleRepository = questionShuffleRepository;
            _questionExplanationErrorRepository = questionExplanationErrorRepository;
            _homeWorkResultRepository = homeWorkResult;
        }

        public async Task<MethodResult<HomeWorkModel>> Handle(GetHomeWorkQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<HomeWorkModel> methodResult = new MethodResult<HomeWorkModel>();
            var studentsResult = await _userService.GetStudentByUserIdWithCacheAsync(_authContext.CurrentUserId);
            if (studentsResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentsResult));
                return methodResult;
            }
            var studentId = studentsResult.Content?.Result?.Id;

            var homeWorkResult = await _homeWorkResultRepository.Queryable.FirstOrDefaultAsync(x => x.Id == request.HomeWorkResultId && x.LessonModuleId == request.LessonModuleId && x.StudentId == studentId, cancellationToken);
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
            methodResult.Result = await GetHomeWorkAsync(homeWork, homeWorkResult, request.IsShowSubStatus);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<HomeWorkModel> GetHomeWorkAsync(HomeWork homeWork, HomeWorkResult homeWorkResult, bool isShowSubStatus)
        {
            var homeWorkModel = _mapper.Map<HomeWorkModel>(homeWork);
            var listQuestionShuffle = new List<QuestionShuffle>();
            var homeWorkQuestions = homeWork.HomeWorkQuestions.OrderBy(x => x!.CreatedDate).ToList();
            var questionIds = homeWorkQuestions.Select(x => x!.QuestionId).ToList();

            var questionShuffles = await _questionShuffleRepository.Queryable.WhereBulkContains(questionIds, x => x.QuestionId)
                                                                             .Where(x => x.StudentId == homeWorkResult.StudentId).ToListAsync();
            var questionExplanationErrors = await _questionExplanationErrorRepository.Queryable.WhereBulkContains(questionIds, x => x.QuestionId)
                                                                                     .Where(x => x.Status == EnumProcessedStatus.NotProcessed && x.ObjectResultId == homeWorkResult.Id)
                                                                                     .ToListAsync();

            foreach (var homeWorkQuestion in homeWorkQuestions)
            {
                var question = homeWorkQuestion.Question;
                if (question == null)
                {
                    continue;
                }
                var homeWorkAnswer = homeWorkQuestion.HomeWorkAnswers.FirstOrDefault(n => n.HomeWorkResultId == homeWorkResult.Id);
                bool isCheck = homeWorkAnswer?.Status == EnumAnswerStatus.Done;
                var questionShuffle = questionShuffles.FirstOrDefault(x => x.QuestionId == question.Id);

                var questionModel = _mapper.Map<QuestionModel>(question);
                questionModel.CorrectStatus = GetCorrectStatus(homeWorkAnswer);
                questionModel.IsReportExplanation = questionExplanationErrors.Any(x => x.QuestionId == question.Id);
                questionModel.Config = _questionTypeConverter.QuestionTypeConverterObject(question.Config, question.QuestionType, isDisableAnswers: !(isCheck)).Item1;
                (questionModel.Config, string? questionShuffleStr) = _questionTypeConverter.QuestionShuffleConverterObject(questionModel.Config, question.QuestionType, questionShuffle?.ShuffleConfigs);
                if (!string.IsNullOrEmpty(questionShuffleStr) && (questionShuffle == null || questionShuffle.ShuffleConfigStr != questionShuffleStr))
                {
                    if (questionShuffle == null)
                    {
                        questionShuffle = new QuestionShuffle
                        {
                            QuestionId = question.Id,
                            StudentId = homeWorkResult.StudentId,
                            ShuffleConfigStr = questionShuffleStr
                        };
                    }
                    else
                    {
                        questionShuffle.ShuffleConfigStr = questionShuffleStr;
                    }
                    listQuestionShuffle.Add(questionShuffle);
                }
                if (homeWorkAnswer != null)
                {
                    homeWorkAnswer.CorrectCount = isCheck ? homeWorkAnswer.CorrectCount : default;
                    homeWorkAnswer.IsCorrect = isCheck ? homeWorkAnswer.IsCorrect : default;
                    homeWorkAnswer.Answer = _answerTypeConverter.AnswerTypeConverterObject(homeWorkAnswer.Answer, question.QuestionType, isShowSubStatus, homeWorkResult.Status, homeWorkResult.SubmissionCount == EnumSubmissionCount.SecondSubmit);
                    questionModel.ResultAnswer = _mapper.Map<AnswerModel>(homeWorkAnswer);
                }

                homeWorkModel.Questions.Add(questionModel);
            }

            await _questionShuffleRepository.SaveQuestionShufflesAsync(listQuestionShuffle);
            homeWorkModel.HomeWorkResult = _mapper.Map<HomeWorkResultModel>(homeWorkResult);
            return homeWorkModel;
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
