// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.HomeWorkExtraCmd
{
    using System.Threading;
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

        public CreateHomeWorkExtraPracticeAnswerCommandHandler(IHomeWorkExtraPracticeResultRepository homeWorkExtraPracticeResultRepository,
            QuestionConverter questionConverter,
            IHomeWorkExtraPracticeAnswerRepository homeWorkExtraPracticeAnswerRepository,
            IHomeWorkRepository homeWorkRepository,
            IMediator mediator,
            IUserService userService,
            AuthContext authContext,
            IQuestionRepository questionRepository,
            ILogger<CreateHomeWorkExtraPracticeAnswerCommand> logger,
            IHomeWorkQuestionRepository homeWorkQuestionRepository)
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
        }

        public async Task<MethodResult<HomeWorkExtraDtoModel>> Handle(CreateHomeWorkExtraPracticeAnswerCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<HomeWorkExtraDtoModel>();

            var moduleResult = await GetHomeWorkExtraPracticeResultAsync(request.HomeWorkExtraPracticeResultId);
            if (!moduleResult.IsOK)
            {
                methodResult.AddErrorBadRequest(moduleResult.ErrorMessages);
                return methodResult;
            }
            var homeWorkExamPracticeResult = moduleResult.Result!;

            if (request.Answers != null && request.Answers.Any())
            {
                var method = await ValidateQuestionsAsync(request, homeWorkExamPracticeResult.HomeWorkId);
                if (!method.IsOK)
                {
                    methodResult.AddErrorBadRequest(method.ErrorMessages);
                    return methodResult;
                }
                var questions = method.Result!;

                var methodSave = await SaveAnswerAsync(homeWorkExamPracticeResult, questions, request, cancellationToken);
                if (!methodSave.IsOK)
                {
                    methodResult.AddErrorBadRequest(methodSave.ErrorMessages);
                    return methodResult;
                }
            }

            var methodHomeWork = await UpdateHomeWorkExtraPracticeResultAsync(homeWorkExamPracticeResult, request.IsSubmit, cancellationToken);
            if (!methodHomeWork.IsOK)
            {
                methodResult.AddErrorBadRequest(methodHomeWork.ErrorMessages);
                return methodResult;
            }

            methodResult = await _mediator.Send(new GetHomeWorkExtraQuery { HomeWorkId = homeWorkExamPracticeResult.HomeWorkId, IsShowSubStatus = request.IsSubmit }, cancellationToken);
            return methodResult;
        }

        private async Task<MethodResult<HomeWorkExtraPracticeResult>> GetHomeWorkExtraPracticeResultAsync(Guid homeWorkExtraPracticeResultId)
        {
            var methodResult = new MethodResult<HomeWorkExtraPracticeResult>();
            var homeWorkExtraPracticeResult = await _homeWorkExtraPracticeResultRepository.GetByIdAsync(homeWorkExtraPracticeResultId);
            if (homeWorkExtraPracticeResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(homeWorkExtraPracticeResult), homeWorkExtraPracticeResultId);
                return methodResult;
            }
            if (homeWorkExtraPracticeResult.Status == EnumResultStatus.Done)
            {
                methodResult.AddErrorBadRequest(nameof(EnumResultErrorCode.ResultStatusDone), nameof(homeWorkExtraPracticeResult.Status), homeWorkExtraPracticeResult.Status);
                return methodResult;
            }

            methodResult.Result = homeWorkExtraPracticeResult;
            return methodResult;
        }

        public async Task<MethodResult<List<Question>>> ValidateQuestionsAsync(CreateHomeWorkExtraPracticeAnswerCommand request, Guid homeworkId)
        {
            ArgumentNullException.ThrowIfNull(request);
            var result = new MethodResult<List<Question>>();

            if (request.Answers == null || !request.Answers.Any())
            {
                return result;
            }

            var questionIds = request.Answers.Select(a => a.QuestionId).ToList();

            var duplicatedIds = questionIds.GroupBy(id => id).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            if (duplicatedIds.Any())
            {
                result.AddErrorBadRequest(nameof(EnumQuestionErrorCode.QuestionsDuplicate), nameof(request.Answers));
                return result;
            }

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

        private async Task<IList<HomeWorkExtraPracticeAnswer>> GetAnswersAsync(IList<Guid> questionIds, Guid homeWorkExtraPracticeResultId)
        {
            return await _homeWorkExtraPracticeAnswerRepository.Queryable.WhereBulkContains(questionIds, x => x.QuestionId)
                                                               .Where(x => x.HomeWorkExtraPracticeResultId == homeWorkExtraPracticeResultId)
                                                               .ToListAsync();
        }

        private async Task<MethodResult<bool>> SaveAnswerAsync(HomeWorkExtraPracticeResult homeWorkExtraPracticeResult, IList<Question>? questions, CreateHomeWorkExtraPracticeAnswerCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request.Answers);
            ArgumentNullException.ThrowIfNull(questions);
            var methodResult = new MethodResult<bool>();
            var isTryAgain = homeWorkExtraPracticeResult.SubmissionCount == EnumSubmissionCount.SecondSubmit;

            var answers = await GetAnswersAsync(questions.Select(x => x.Id).ToList(), homeWorkExtraPracticeResult.Id);

            var createHomeWorkAnswers = new List<HomeWorkExtraPracticeAnswer>();
            var updateHomeWorkAnswers = new List<HomeWorkExtraPracticeAnswer>();
            foreach (var item in request.Answers)
            {
                var question = questions.FirstOrDefault(x => x.Id == item.QuestionId);
                if (question == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(question), item.QuestionId);
                    return methodResult;
                }
                var homeWorkExtraPracticeAnswer = answers.FirstOrDefault(x => x.QuestionId == item.QuestionId) ?? new HomeWorkExtraPracticeAnswer { QuestionId = question.Id, HomeWorkExtraPracticeResultId = homeWorkExtraPracticeResult.Id };
                if (homeWorkExtraPracticeAnswer.Id == Guid.Empty)
                {
                    createHomeWorkAnswers.Add(homeWorkExtraPracticeAnswer);
                }
                else if (homeWorkExtraPracticeAnswer.Status != EnumAnswerStatus.Done)
                {
                    updateHomeWorkAnswers.Add(homeWorkExtraPracticeAnswer);
                }

                var questionResult = _questionConverter.HandleQuestionAnswer(question, item.Answer, request.IsSubmit, homeWorkExtraPracticeAnswer.Answer, isTryAgain, request.IsSubmit);
                if (!questionResult.IsOK)
                {
                    methodResult.AddErrorBadRequest(questionResult.ErrorMessages);
                    return methodResult;
                }
                var (questionItem, answerConfig, correctCount, isAnswered) = questionResult.Result;

                homeWorkExtraPracticeAnswer.Status = EnumAnswerStatus.Process;
                homeWorkExtraPracticeAnswer.Answer = answerConfig;
                homeWorkExtraPracticeAnswer.CorrectCount = correctCount;
                homeWorkExtraPracticeAnswer.IsCorrect = isAnswered ? correctCount == question.CorrectTotal : null;

                if (!homeWorkExtraPracticeAnswer.IsValid())
                {
                    methodResult.AddErrorBadRequest(homeWorkExtraPracticeAnswer.ErrorMessages);
                    return methodResult;
                }
            }
            await SaveAnswersAsync(createHomeWorkAnswers, updateHomeWorkAnswers);
            methodResult.Result = true;
            return methodResult;
        }

        private async Task SaveAnswersAsync(List<HomeWorkExtraPracticeAnswer> creates, List<HomeWorkExtraPracticeAnswer> updates)
        {
            try
            {
                if (creates.Any())
                {
                    await _homeWorkExtraPracticeAnswerRepository.BulkMergeAsync(creates, bulk =>
                    {
                        bulk.ColumnPrimaryKeyExpression = e => new { e.QuestionId, e.HomeWorkExtraPracticeResultId, e.IsDeleted };
                    });
                }

                if (updates.Any())
                {
                    await _homeWorkExtraPracticeAnswerRepository.BulkUpdateList(updates, bulk =>
                    {
                        bulk.IgnoreOnUpdateExpression = e => new { e.QuestionId, e.HomeWorkExtraPracticeResultId };
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Log Duplicate HomeWorkAnswer : {ex.Message}");
            }
        }

        private async Task<MethodResult<bool>> UpdateHomeWorkExtraPracticeResultAsync(HomeWorkExtraPracticeResult homeWorkResult, bool isSubmit, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(homeWorkResult);
            var methodResult = new MethodResult<bool>();

            var homeWorkQuestionCount = await _homeWorkExtraPracticeResultRepository.Queryable.Where(x => x.Id == homeWorkResult.Id).Select(x => new
            {
                CourseSkill = x.HomeWork!.CourseSkill,
                CourseLevel = x.HomeWork.CourseLevel,
                CorrectCount = x.HomeWorkExtraPracticeAnswers.Sum(x => x.CorrectCount),
                CorrectTotal = x.HomeWork.HomeWorkQuestions.Select(x => x.Question).Sum(x => x!.CorrectTotal),
                TotalQuestion = x.HomeWork.HomeWorkQuestions.Count,
                TotalAnswer = x.HomeWorkExtraPracticeAnswers.Count,
            }).FirstOrDefaultAsync(cancellationToken);

            if (homeWorkQuestionCount == null)
            {
                return methodResult;
            }

            if (homeWorkResult.Status == EnumResultStatus.New)
            {
                homeWorkResult.Status = EnumResultStatus.Process;
            }
            if (isSubmit)
            {
                var isHomeWorkDone = homeWorkQuestionCount.CorrectCount == homeWorkQuestionCount.CorrectTotal || homeWorkResult.SubmissionCount == EnumSubmissionCount.SecondSubmit;
                if (homeWorkQuestionCount.TotalAnswer > homeWorkQuestionCount.TotalQuestion)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumAnswerErrorCode.DuplicateAnswers));
                    return methodResult;
                }
                if (homeWorkQuestionCount.TotalAnswer < homeWorkQuestionCount.TotalQuestion)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumAnswerErrorCode.NotAnsweredEnough));
                    return methodResult;
                }
                await UpdateAnswersAsync(homeWorkResult, isHomeWorkDone);
                SetHomeWorkExtraPracticeResult(homeWorkResult, homeWorkQuestionCount, isHomeWorkDone);
            }

            await _homeWorkExtraPracticeResultRepository.BulkUpdateList(new List<HomeWorkExtraPracticeResult> { homeWorkResult }, bulk =>
            {
                bulk.IgnoreOnUpdateExpression = c => new { c.StudentId, c.HomeWorkId };
            });
            await _homeWorkExtraPracticeResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            methodResult.Result = true;
            return methodResult;
        }

        private static void SetHomeWorkExtraPracticeResult(HomeWorkExtraPracticeResult homeWorkResult, dynamic homeWorkQuestionCount, bool isHomeWorkDone)
        {
            homeWorkResult.CorrectCount = homeWorkQuestionCount.CorrectCount;
            homeWorkResult.CorrectTotal = homeWorkQuestionCount.CorrectTotal;
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
                    Skill = homeWorkQuestionCount.CourseSkill,
                    CorrectCount = homeWorkQuestionCount.CorrectCount,
                    TotalCount = homeWorkQuestionCount.CorrectTotal,
                    CountQuestion = homeWorkQuestionCount.TotalAnswer,
                    TotalQuestion = homeWorkQuestionCount.TotalQuestion,
                }
            };
        }

        public async Task UpdateAnswersAsync(HomeWorkExtraPracticeResult? homeWorkResult, bool isDone = false)
        {
            ArgumentNullException.ThrowIfNull(homeWorkResult);
            var answerQuestions = await (from baseQ in _homeWorkExtraPracticeAnswerRepository.Queryable
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
                    answer.IsCorrect = answer.IsCorrect.HasValue ? (answer.CorrectCount == x.CorrectTotal) : null;
                }
                toUpdate.Add(answer);
            }

            await _homeWorkExtraPracticeAnswerRepository.BulkUpdateList(toUpdate, bulk =>
            {
                bulk.IgnoreOnUpdateExpression = e => new { e.HomeWorkExtraPracticeResultId, e.QuestionId };
            });
        }
    }
}
