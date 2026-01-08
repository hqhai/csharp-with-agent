// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.HomeWorkExtraCmd
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.SkillScoresConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.HomeWorkAnswers;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Queries.HomeWorkExtraQuery;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    public class CreateHomeWorkExtraPracticeAnswerCommand : CreateHomeWorkExtraPracticeAnswerCommandModel, IRequest<MethodResult<HomeWorkExtraDtoModel>>
    {
    }

    public class CreateHomeWorkExtraPracticeAnswerCommandHandler : IRequestHandler<CreateHomeWorkExtraPracticeAnswerCommand, MethodResult<HomeWorkExtraDtoModel>>
    {
        private readonly IHomeWorkExtraPracticeResultRepository _homeWorkExtraPracticeResultRepository;
        private readonly QuestionConverter _questionConverter;
        private readonly IHomeWorkExtraPracticeAnswerRepository _homeWorkExtraPracticeAnswerRepository;
        private readonly IHomeWorkRepository _homeWorkRepository;
        private readonly IMediator _mediator;
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;
        private readonly IQuestionRepository _questionRepository;
        private readonly ILogger<CreateHomeWorkExtraPracticeAnswerCommand> _logger;
        private readonly IHomeWorkQuestionRepository _homeWorkQuestionRepository;
        private readonly IHomeWorkRetryRepository _homeWorkRetryRepository;

        public CreateHomeWorkExtraPracticeAnswerCommandHandler(
            IHomeWorkExtraPracticeResultRepository homeWorkExtraPracticeResultRepository,
            QuestionConverter questionConverter,
            IHomeWorkExtraPracticeAnswerRepository homeWorkExtraPracticeAnswerRepository,
            IHomeWorkRepository homeWorkRepository,
            IMediator mediator,
            IUserService userService,
            AuthContext authContext,
            IQuestionRepository questionRepository,
            ILogger<CreateHomeWorkExtraPracticeAnswerCommand> logger,
            IHomeWorkQuestionRepository homeWorkQuestionRepository,
            IHomeWorkRetryRepository homeWorkRetryRepository)
        {
            _homeWorkExtraPracticeResultRepository = homeWorkExtraPracticeResultRepository;
            _homeWorkExtraPracticeAnswerRepository = homeWorkExtraPracticeAnswerRepository;
            _questionConverter = questionConverter;
            _homeWorkRepository = homeWorkRepository;
            _mediator = mediator;
            _userService = userService;
            _authContext = authContext;
            _questionRepository = questionRepository;
            _logger = logger;
            _homeWorkQuestionRepository = homeWorkQuestionRepository;
            _homeWorkRetryRepository = homeWorkRetryRepository;
        }

        public async Task<MethodResult<HomeWorkExtraDtoModel>> Handle(CreateHomeWorkExtraPracticeAnswerCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<HomeWorkExtraDtoModel>();

            // 1. Lấy kết quả ExtraPractice
            var moduleResult = await GetHomeWorkExtraPracticeResultAsync(request.HomeWorkExtraPracticeResultId);
            if (!moduleResult.IsOK)
            {
                methodResult.AddErrorBadRequest(moduleResult.ErrorMessages);
                return methodResult;
            }
            var homeWorkExtraPracticeResult = moduleResult.Result!;

            // 2. Lấy HomeWorkRetry để lấy config
            var moduleResultRetry = await GetHomeWorkRetryAsync(homeWorkExtraPracticeResult.HomeWorkRetryId);
            if (!moduleResultRetry.IsOK)
            {
                methodResult.AddErrorBadRequest(moduleResultRetry.ErrorMessages);
                return methodResult;
            }
            var homeWorkRetry = moduleResultRetry.Result!;

            // 3. Nếu có Answers thì validate + lưu
            if (request.Answers != null && request.Answers.Any())
            {
                var validateQuestionsResult = await ValidateQuestionsAsync(request, homeWorkExtraPracticeResult.HomeWorkId);
                if (!validateQuestionsResult.IsOK)
                {
                    methodResult.AddErrorBadRequest(validateQuestionsResult.ErrorMessages);
                    return methodResult;
                }
                var questions = validateQuestionsResult.Result!;

                var saveAnswersResult = await SaveAnswerAsync(homeWorkExtraPracticeResult, questions, request, cancellationToken);
                if (!saveAnswersResult.IsOK)
                {
                    methodResult.AddErrorBadRequest(saveAnswersResult.ErrorMessages);
                    return methodResult;
                }
            }

            // 4. Update kết quả ExtraPractice (status, scores, HighestStreak)
            var updateResult = await UpdateHomeWorkExtraPracticeResultAsync(homeWorkExtraPracticeResult, request.IsSubmit, cancellationToken);
            if (!updateResult.IsOK)
            {
                methodResult.AddErrorBadRequest(updateResult.ErrorMessages);
                return methodResult;
            }

            // 5. Get DTO trả về
            methodResult = await _mediator.Send(
                new GetHomeWorkExtraQuery
                {
                    Id = homeWorkExtraPracticeResult.HomeWorkId,
                    IsShowSubStatus = request.IsSubmit,
                    HomeWorkConfigId = homeWorkRetry.HomeWorkConfigId
                },
                cancellationToken);

            return methodResult;
        }

        #region Load Result & Retry

        private async Task<MethodResult<HomeWorkExtraPracticeResult>> GetHomeWorkExtraPracticeResultAsync(Guid homeWorkExtraPracticeResultId)
        {
            var methodResult = new MethodResult<HomeWorkExtraPracticeResult>();
            var homeWorkExtraPracticeResult = await _homeWorkExtraPracticeResultRepository.GetByIdAsync(homeWorkExtraPracticeResultId);

            if (homeWorkExtraPracticeResult == null)
            {
                methodResult.AddErrorBadRequest(
                    nameof(EnumSystemErrorCode.DataNotExist),
                    nameof(homeWorkExtraPracticeResult),
                    homeWorkExtraPracticeResultId);
                return methodResult;
            }

            if (homeWorkExtraPracticeResult.Status == EnumResultStatus.Done)
            {
                methodResult.AddErrorBadRequest(
                    nameof(EnumResultErrorCode.ResultStatusDone),
                    nameof(homeWorkExtraPracticeResult.Status),
                    homeWorkExtraPracticeResult.Status);
                return methodResult;
            }

            methodResult.Result = homeWorkExtraPracticeResult;
            return methodResult;
        }

        private async Task<MethodResult<HomeWorkRetry>> GetHomeWorkRetryAsync(Guid homeWorkRetryId)
        {
            var methodResult = new MethodResult<HomeWorkRetry>();
            var homeWorkRetry = await _homeWorkRetryRepository.GetByIdAsync(homeWorkRetryId);

            if (homeWorkRetry == null)
            {
                methodResult.AddErrorBadRequest(
                    nameof(EnumSystemErrorCode.DataNotExist),
                    nameof(homeWorkRetry),
                    homeWorkRetryId);
                return methodResult;
            }

            methodResult.Result = homeWorkRetry;
            return methodResult;
        }

        #endregion Load Result & Retry

        #region Validate Questions

        public async Task<MethodResult<List<Question>>> ValidateQuestionsAsync(
            CreateHomeWorkExtraPracticeAnswerCommand request,
            Guid homeworkId)
        {
            ArgumentNullException.ThrowIfNull(request);
            var result = new MethodResult<List<Question>>();

            if (request.Answers == null || !request.Answers.Any())
            {
                return result;
            }

            var questionIds = request.Answers.Select(a => a.QuestionId).ToList();

            // Trùng câu trong request
            var duplicatedIds = questionIds
                .GroupBy(id => id)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicatedIds.Any())
            {
                result.AddErrorBadRequest(nameof(EnumQuestionErrorCode.QuestionsDuplicate), nameof(request.Answers));
                return result;
            }

            // Câu hỏi phải thuộc về homework
            var validQuestionIds = await _homeWorkQuestionRepository.Queryable
                .AsNoTracking()
                .Where(x => x.HomeWorkId == homeworkId)
                .Select(x => x.QuestionId)
                .ToListAsync();

            var validSet = new HashSet<Guid>(validQuestionIds);
            var notBelongIds = questionIds
                .Where(id => !validSet.Contains(id))
                .Distinct()
                .ToList();

            if (notBelongIds.Count > 0)
            {
                result.AddErrorBadRequest(nameof(EnumSystemErrorCode.InValidFormat), nameof(request.Answers));
                return result;
            }

            var questions = await _questionRepository.GetByIdsAsync(validQuestionIds);
            if (questions == null || !questions.Any())
            {
                result.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(questions));
                return result;
            }

            result.Result = questions.ToList();
            return result;
        }

        #endregion Validate Questions

        #region Save Answers

        private async Task<IList<HomeWorkExtraPracticeAnswer>> GetAnswersAsync(
            IList<Guid> questionIds,
            Guid homeWorkExtraPracticeResultId)
        {
            return await _homeWorkExtraPracticeAnswerRepository.Queryable
                .WhereBulkContains(questionIds, x => x.QuestionId)
                .Where(x => x.HomeWorkExtraPracticeResultId == homeWorkExtraPracticeResultId)
                .ToListAsync();
        }

        private async Task<MethodResult<bool>> SaveAnswerAsync(
            HomeWorkExtraPracticeResult homeWorkExtraPracticeResult,
            IList<Question>? questions,
            CreateHomeWorkExtraPracticeAnswerCommand request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request.Answers);
            ArgumentNullException.ThrowIfNull(questions);

            var methodResult = new MethodResult<bool>();

            var isFirstSubmit = homeWorkExtraPracticeResult.SubmissionCount == EnumSubmissionCount.FirstSubmit;
            var isTryAgain = homeWorkExtraPracticeResult.SubmissionCount == EnumSubmissionCount.SecondSubmit;

            var answers = await GetAnswersAsync(questions.Select(x => x.Id).ToList(), homeWorkExtraPracticeResult.Id);

            var createHomeWorkAnswers = new List<HomeWorkExtraPracticeAnswer>();
            var updateHomeWorkAnswers = new List<HomeWorkExtraPracticeAnswer>();

            foreach (var item in request.Answers)
            {
                var question = questions.FirstOrDefault(x => x.Id == item.QuestionId);
                if (question == null)
                {
                    methodResult.AddErrorBadRequest(
                        nameof(EnumSystemErrorCode.DataNotExist),
                        nameof(question),
                        item.QuestionId);
                    return methodResult;
                }

                var answer = answers.FirstOrDefault(x => x.QuestionId == item.QuestionId)
                    ?? new HomeWorkExtraPracticeAnswer
                    {
                        QuestionId = question.Id,
                        HomeWorkExtraPracticeResultId = homeWorkExtraPracticeResult.Id
                    };

                var isNewAnswer = answer.Id == Guid.Empty;

                if (isNewAnswer)
                {
                    createHomeWorkAnswers.Add(answer);
                }
                else if (answer.Status != EnumAnswerStatus.Done)
                {
                    updateHomeWorkAnswers.Add(answer);
                }

                var questionResult = _questionConverter.HandleQuestionAnswer(
                    question,
                    item.Answer,
                    request.IsSubmit,
                    answer.Answer,
                    isTryAgain,
                    request.IsSubmit);

                if (!questionResult.IsOK)
                {
                    methodResult.AddErrorBadRequest(questionResult.ErrorMessages);
                    return methodResult;
                }

                var (questionItem, answerConfig, correctCount, isAnswered) = questionResult.Result;

                BuildHomeWorkExtraPracticeAnswer(
                    answer,
                    answerConfig,
                    isAnswered,
                    correctCount,
                    questionItem.CorrectTotal,
                    isFirstSubmit);

                if (!answer.IsValid())
                {
                    methodResult.AddErrorBadRequest(answer.ErrorMessages);
                    return methodResult;
                }
            }

            await SaveAnswersAsync(createHomeWorkAnswers, updateHomeWorkAnswers);
            methodResult.Result = true;
            return methodResult;
        }

        private static void BuildHomeWorkExtraPracticeAnswer(
            HomeWorkExtraPracticeAnswer answer,
            object? answerConfig,
            bool isAnswered,
            short correctCount,
            int correctTotal,
            bool isFirstSubmit)
        {
            answer.Status = EnumAnswerStatus.Process;
            answer.Answer = answerConfig;
            answer.CorrectCount = correctCount;
            answer.IsCorrect = isAnswered ? correctCount == correctTotal : null;
            answer.IsFirstSubmit = isFirstSubmit;
        }

        private async Task SaveAnswersAsync(List<HomeWorkExtraPracticeAnswer> creates, List<HomeWorkExtraPracticeAnswer> updates)
        {
            try
            {
                if (creates.Any())
                {
                    await _homeWorkExtraPracticeAnswerRepository.BulkMergeAsync(creates, bulk =>
                    {
                        bulk.ColumnPrimaryKeyExpression = e => new
                        {
                            e.QuestionId,
                            e.HomeWorkExtraPracticeResultId,
                            e.IsDeleted
                        };
                    });
                }

                if (updates.Any())
                {
                    await _homeWorkExtraPracticeAnswerRepository.BulkUpdateList(updates, bulk =>
                    {
                        bulk.IgnoreOnUpdateExpression = e => new
                        {
                            e.QuestionId,
                            e.HomeWorkExtraPracticeResultId
                        };
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Log Duplicate HomeWorkAnswer : {ex.Message}");
            }
        }

        #endregion Save Answers

        #region Update Result & Statistics

        private sealed class HomeWorkExtraPracticeStatistics
        {
            public Guid? SkillId { get; init; }
            public string? SkillName { get; init; }
            public string? SkillFilePath { get; init; }
            public EnumCourseSkill CourseSkill { get; init; }
            public EnumCourseLevel CourseLevel { get; init; }
            public int CorrectQuestion { get; init; }
            public int CorrectCount { get; init; }
            public int CorrectTotal { get; init; }
            public int TotalQuestion { get; init; }
            public int TotalAnswer { get; init; }
        }

        private async Task<HomeWorkExtraPracticeStatistics?> GetHomeWorkExtraPracticeStatisticsAsync(
            Guid homeWorkExtraPracticeResultId,
            CancellationToken cancellationToken)
        {
            return await _homeWorkExtraPracticeResultRepository.ReadQueryable
                .Where(x => x.Id == homeWorkExtraPracticeResultId)
                .Select(x => new HomeWorkExtraPracticeStatistics
                {
                    SkillId = x.HomeWork != null ? x.HomeWork.SkillId : null,
                    SkillName = x.HomeWork != null && x.HomeWork.Skill != null ? x.HomeWork.Skill.Name : string.Empty,
                    SkillFilePath = x.HomeWork != null && x.HomeWork.Skill != null ? x.HomeWork.Skill.FilePath : string.Empty,
                    CourseSkill = x.HomeWork!.CourseSkill,
                    CourseLevel = x.HomeWork.CourseLevel,
                    CorrectQuestion = x.HomeWorkExtraPracticeAnswers.Count(a => a.IsCorrect == true),
                    CorrectCount = x.HomeWorkExtraPracticeAnswers.Sum(a => a.CorrectCount),
                    CorrectTotal = x.HomeWork.HomeWorkQuestions
                        .Select(hq => hq.Question)
                        .Sum(q => q!.CorrectTotal),
                    TotalQuestion = x.HomeWork.HomeWorkQuestions.Count,
                    TotalAnswer = x.HomeWorkExtraPracticeAnswers.Count
                })
                .FirstOrDefaultAsync(cancellationToken);
        }

        private async Task<MethodResult<bool>> UpdateHomeWorkExtraPracticeResultAsync(
            HomeWorkExtraPracticeResult homeWorkResult,
            bool isSubmit,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(homeWorkResult);
            var methodResult = new MethodResult<bool>();

            var statistics = await GetHomeWorkExtraPracticeStatisticsAsync(homeWorkResult.Id, cancellationToken);
            if (statistics == null)
            {
                return methodResult;
            }

            if (homeWorkResult.Status == EnumResultStatus.New)
            {
                homeWorkResult.Status = EnumResultStatus.Process;
            }

            if (isSubmit)
            {
                var isHomeWorkDone =
                    statistics.CorrectCount == statistics.CorrectTotal ||
                    homeWorkResult.SubmissionCount == EnumSubmissionCount.SecondSubmit;

                if (statistics.TotalAnswer > statistics.TotalQuestion)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumAnswerErrorCode.DuplicateAnswers));
                    return methodResult;
                }

                if (statistics.TotalAnswer < statistics.TotalQuestion)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumAnswerErrorCode.NotAnsweredEnough));
                    return methodResult;
                }

                // Chốt trạng thái các câu trả lời
                await UpdateAnswersAsync(homeWorkResult, isHomeWorkDone);

                // Tính HighestStreak cho ExtraPractice (chỉ tính FirstSubmit)
                homeWorkResult.HighestStreak = await CalculateHighestCorrectStreakAsync(homeWorkResult.Id, cancellationToken);

                // Cập nhật thống kê vào result
                SetHomeWorkExtraPracticeResult(homeWorkResult, statistics, isHomeWorkDone);
            }

            await _homeWorkExtraPracticeResultRepository.BulkUpdateList(
                new List<HomeWorkExtraPracticeResult> { homeWorkResult },
                bulk =>
                {
                    bulk.IgnoreOnUpdateExpression = c => new { c.StudentId, c.HomeWorkId };
                });

            methodResult.Result = true;
            return methodResult;
        }

        private static void SetHomeWorkExtraPracticeResult(
            HomeWorkExtraPracticeResult homeWorkResult,
            HomeWorkExtraPracticeStatistics statistics,
            bool isHomeWorkDone)
        {
            homeWorkResult.CorrectCount = statistics.CorrectCount;
            homeWorkResult.CorrectTotal = statistics.CorrectTotal;

            if (isHomeWorkDone)
            {
                homeWorkResult.Status = EnumResultStatus.Done;
            }
            else
            {
                homeWorkResult.SubmissionCount = EnumSubmissionCount.SecondSubmit;
            }

            homeWorkResult.SkillScores = new List<SkillScores>
            {
                new SkillScores
                {
                    Skill = statistics.CourseSkill,
                    CorrectCount = statistics.CorrectCount,
                    TotalCount = statistics.CorrectTotal,
                    CountQuestion = statistics.TotalAnswer,
                    TotalQuestion = statistics.TotalQuestion,
                    SkillFilePath = statistics.SkillFilePath,
                    SkillName = statistics.SkillName,
                    SkillId = statistics.SkillId,
                    CorrectQuestion = statistics.CorrectQuestion,
                }
            };
        }

        #endregion Update Result & Statistics

        #region Finalize Answers & HighestStreak

        public async Task UpdateAnswersAsync(HomeWorkExtraPracticeResult? homeWorkResult, bool isDone = false)
        {
            ArgumentNullException.ThrowIfNull(homeWorkResult);

            var answerQuestions = await (
                from baseQ in _homeWorkExtraPracticeAnswerRepository.Queryable
                join q in _questionRepository.Queryable on baseQ.QuestionId equals q.Id
                where baseQ.HomeWorkExtraPracticeResultId == homeWorkResult.Id
                      && baseQ.Status == EnumAnswerStatus.Process
                select new
                {
                    HomeWorkExtraPracticeAnswer = baseQ,
                    CorrectTotal = q.CorrectTotal
                }).ToListAsync();

            var toUpdate = new List<HomeWorkExtraPracticeAnswer>();

            foreach (var x in answerQuestions)
            {
                var answer = x.HomeWorkExtraPracticeAnswer;
                var willBeDone = (answer.CorrectCount == x.CorrectTotal) || isDone;
                var newStatus = willBeDone ? EnumAnswerStatus.Done : EnumAnswerStatus.Process;

                if (answer.Status != newStatus || answer.IsCorrect.HasValue)
                {
                    answer.Status = newStatus;
                    answer.IsCorrect = answer.IsCorrect.HasValue
                        ? (answer.CorrectCount == x.CorrectTotal)
                        : (bool?)null;
                }

                toUpdate.Add(answer);
            }

            if (toUpdate.Any())
            {
                await _homeWorkExtraPracticeAnswerRepository.BulkUpdateList(toUpdate, bulk =>
                {
                    bulk.ColumnInputExpression = entity => new
                    {
                        entity.Status,
                        entity.IsCorrect
                    };
                });
            }
        }

        private async Task<int> CalculateHighestCorrectStreakAsync(
            Guid homeWorkExtraPracticeResultId,
            CancellationToken cancellationToken)
        {
            // HighestStreak: chuỗi dài nhất các câu đúng (IsCorrect == true) liên tiếp,
            // chỉ tính các answer FirstSubmit và đã Done.
            var answers = await _homeWorkExtraPracticeAnswerRepository.ReadQueryable
                .Where(x => x.HomeWorkExtraPracticeResultId == homeWorkExtraPracticeResultId)
                .Where(x => x.Status == EnumAnswerStatus.Done && x.IsFirstSubmit)
                .OrderBy(x => x.CreatedDate)
                .Select(x => x.IsCorrect)
                .ToListAsync(cancellationToken);

            if (!answers.Any())
            {
                return 0;
            }

            var currentStreak = 0;
            var maxStreak = 0;

            foreach (var isCorrect in answers)
            {
                if (isCorrect == true)
                {
                    currentStreak++;
                    if (currentStreak > maxStreak)
                    {
                        maxStreak = currentStreak;
                    }
                }
                else
                {
                    currentStreak = 0;
                }
            }

            return maxStreak;
        }

        #endregion Finalize Answers & HighestStreak
    }
}
