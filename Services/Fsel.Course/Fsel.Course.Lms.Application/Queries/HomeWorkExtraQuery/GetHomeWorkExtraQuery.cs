// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.HomeWorkExtraQuery
{
    using System.Text.Json.Serialization;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Course.Lms.Application.Services.UserServices.Models;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetHomeWorkExtraQuery : IRequest<MethodResult<HomeWorkExtraDtoModel>>
    {
        public Guid Id { get; set; }
        public Guid? HomeWorkConfigId { get; set; }

        [JsonIgnore]
        public bool IsShowSubStatus { get; set; }
    }

    public class GetHomeWorkExtraQueryHandler : IRequestHandler<GetHomeWorkExtraQuery, MethodResult<HomeWorkExtraDtoModel>>
    {
        private readonly IMapper _mapper;
        private readonly IHomeWorkRepository _homeWorkRepository;
        private readonly AuthContext _authContext;
        private readonly QuestionTypeConverter _questionTypeConverter;
        private readonly AnswerTypeConverter _answerTypeConverter;
        private readonly IUserService _userService;
        private readonly IQuestionShuffleRepository _questionShuffleRepository;
        private readonly IQuestionExplanationErrorRepository _questionExplanationErrorRepository;
        private readonly IHomeWorkRetryRepository _homeWorkRetryRepository;
        private readonly IHomeWorkExtraPracticeResultRepository _homeWorkExtraPracticeResultRepository;

        public GetHomeWorkExtraQueryHandler(IMapper mapper
            , IHomeWorkRepository homeWorkRepository
            , AuthContext authContext
            , QuestionTypeConverter questionTypeConverter
            , AnswerTypeConverter answerTypeConverter
            , IHomeWorkExtraPracticeResultRepository homeWorkExtraPracticeResult
            , IUserService userService
            , IQuestionShuffleRepository questionShuffleRepository
            , IQuestionExplanationErrorRepository questionExplanationErrorRepository
            , IHomeWorkRetryRepository homeWorkRetryRepository)
        {
            _mapper = mapper;
            _homeWorkRepository = homeWorkRepository;
            _authContext = authContext;
            _questionTypeConverter = questionTypeConverter;
            _answerTypeConverter = answerTypeConverter;
            _userService = userService;
            _questionShuffleRepository = questionShuffleRepository;
            _questionExplanationErrorRepository = questionExplanationErrorRepository;
            _homeWorkRetryRepository = homeWorkRetryRepository;
            _homeWorkExtraPracticeResultRepository = homeWorkExtraPracticeResult;
        }

        public async Task<MethodResult<HomeWorkExtraDtoModel>> Handle(GetHomeWorkExtraQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<HomeWorkExtraDtoModel> methodResult = new MethodResult<HomeWorkExtraDtoModel>();

            var studentResult = await GetStudentAsync();
            if (!studentResult.IsOK)
            {
                methodResult.AddErrorBadRequest(studentResult.ErrorMessages);
                return methodResult;
            }
            var student = studentResult.Result!;

            var homeWorkRetry = await _homeWorkRetryRepository.Queryable.Where(x => x.HomeWorkId == request.Id && x.StudentId == student.Id)
                                                              .FirstOrDefaultAsync(x => x.HomeWorkConfigId == request.HomeWorkConfigId, cancellationToken);
            if (homeWorkRetry == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(homeWorkRetry));
                return methodResult;
            }

            var result = await _homeWorkExtraPracticeResultRepository.Queryable.Where(x => x.HomeWorkRetryId == homeWorkRetry.Id)
                                                            .Where(x => x.HomeWorkId == request.Id && x.StudentId == student.Id)
                                                            .FirstOrDefaultAsync(x => x.WorkingStatus == EnumWorkingStatus.Active, cancellationToken);
            if (result == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(result));
                return methodResult;
            }

            var homeWork = await GetHomeWorkAsync(result);
            if (homeWork == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(homeWork));
                return methodResult;
            }
            methodResult.Result = await GetHomeWorkAsync(homeWork, result, request.IsShowSubStatus);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        public async Task<HomeWork?> GetHomeWorkAsync(HomeWorkExtraPracticeResult result)
        {
            try
            {
                return await _homeWorkRepository.Queryable
                        .Include(x => x.HomeWorkQuestions)
                        .ThenInclude(x => x.Question)
                        .ThenInclude(x => x.HomeWorkExtraPracticeAnswers.Where(n => n.HomeWorkExtraPracticeResultId == result.Id))
                        .Where(x => x.Id == result.HomeWorkId)
                        .AsNoTracking()
                        .FirstOrDefaultAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task<MethodResult<StudentModel>> GetStudentAsync()
        {
            var methodResult = new MethodResult<StudentModel>();
            var studentResult = await _userService.GetStudentByUserIdWithCacheAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return methodResult;
            }

            var student = studentResult?.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }
            methodResult.Result = student;
            return methodResult;
        }

        private async Task<HomeWorkExtraDtoModel> GetHomeWorkAsync(HomeWork homeWork, HomeWorkExtraPracticeResult result, bool isShowSubStatus)
        {
            var homeWorkModel = _mapper.Map<HomeWorkExtraDtoModel>(homeWork);
            var listQuestionShuffle = new List<QuestionShuffle>();
            var homeWorkQuestions = homeWork.HomeWorkQuestions.OrderBy(x => x!.CreatedDate).ToList();
            var questionIds = homeWorkQuestions.Select(x => x!.QuestionId).ToList();

            var questionShuffles = await _questionShuffleRepository.Queryable.WhereBulkContains(questionIds, x => x.QuestionId)
                                                                             .Where(x => x.StudentId == result.StudentId).ToListAsync();
            var questionExplanationErrors = await _questionExplanationErrorRepository.Queryable.WhereBulkContains(questionIds, x => x.QuestionId)
                                                                                               .Where(x => x.Status == EnumProcessedStatus.NotProcessed && x.ObjectResultId == result.Id).ToListAsync();

            foreach (var homeWorkQuestion in homeWorkQuestions)
            {
                var question = homeWorkQuestion.Question;
                if (question == null)
                {
                    continue;
                }
                var answer = question.HomeWorkExtraPracticeAnswers.FirstOrDefault(n => n.HomeWorkExtraPracticeResultId == result.Id);
                bool isCheck = answer?.Status == EnumAnswerStatus.Done;
                var questionShuffle = questionShuffles.FirstOrDefault(x => x.QuestionId == question.Id);

                var questionModel = _mapper.Map<QuestionModel>(question);
                questionModel.CorrectStatus = GetCorrectStatus(answer);
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
                            StudentId = result.StudentId,
                            ShuffleConfigStr = questionShuffleStr
                        };
                    }
                    else
                    {
                        questionShuffle.ShuffleConfigStr = questionShuffleStr;
                    }
                    listQuestionShuffle.Add(questionShuffle);
                }
                if (answer != null)
                {
                    answer.CorrectCount = isCheck ? answer.CorrectCount : default;
                    answer.IsCorrect = isCheck ? answer.IsCorrect : default;
                    answer.Answer = _answerTypeConverter.AnswerTypeConverterObject(answer.Answer, question.QuestionType, isShowSubStatus, result.Status, result.SubmissionCount == EnumSubmissionCount.SecondSubmit);
                    questionModel.ResultAnswer = _mapper.Map<AnswerModel>(answer);
                }

                homeWorkModel.Questions.Add(questionModel);
            }

            await _questionShuffleRepository.SaveQuestionShufflesAsync(listQuestionShuffle);
            homeWorkModel.HomeWorkExtraPracticeResult = _mapper.Map<HomeWorkExtraPracticeResultModel>(result);
            return homeWorkModel;
        }

        private static EnumCorrectStatus? GetCorrectStatus(HomeWorkExtraPracticeAnswer? homeWorkExtraPracticeAnswer)
        {
            if (homeWorkExtraPracticeAnswer == null)
            {
                return default;
            }
            return homeWorkExtraPracticeAnswer.Status == EnumAnswerStatus.Done
                ? (homeWorkExtraPracticeAnswer.IsCorrect.HasValue && homeWorkExtraPracticeAnswer.IsCorrect.Value
                ? EnumCorrectStatus.Correct : EnumCorrectStatus.Fail) : EnumCorrectStatus.Process;
        }
    }
}
