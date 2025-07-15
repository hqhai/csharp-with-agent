namespace Fsel.System.Application.Commands.DailyQuiz
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.System.Application.Services.UserServices;
    using Fsel.System.Domain.Entities.DailyQuiz;
    using Fsel.System.Domain.Enums;
    using Fsel.System.Domain.IRepositories.DailyQuizs;
    using Fsel.System.Domain.Models.CommandModels.DailyQuiz;
    using Fsel.System.Domain.Models.EntityModels;
    using Fsel.System.Infrastructure.ValueSettings;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class DailyQuizCommand : DailyQuizCommandModels, IRequest<MethodResult<DailyQuizModel>>
    {
    }

    public class DailyQuizCommandHandler : IRequestHandler<DailyQuizCommand, MethodResult<DailyQuizModel>>
    {
        private readonly IDailyQuizHistoryRepository _dailyQuizHistoryRepository;
        private readonly IDailyQuizAnswerRepository _dailyQuizAnswerRepository;
        private readonly IDailyQuizQuestionRepository _dailyQuizQuestionRepository;
        private readonly IDailyQuizWinnerRepository _dailyQuizWinnerRepository;
        private readonly AppSetting _appSetting;
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;
        private readonly IMapper _mapper;
        private const string DefaultAcceptLanguage = "en-US";

        public DailyQuizCommandHandler(IDailyQuizHistoryRepository dailyQuizHistoryRepository, IDailyQuizAnswerRepository dailyQuizAnswerRepository, IDailyQuizQuestionRepository dailyQuizQuestionRepository, IDailyQuizWinnerRepository dailyQuizWinnerRepository, AppSetting appSetting, IUserService userService, AuthContext authContext, IMapper mapper)
        {
            _dailyQuizHistoryRepository = dailyQuizHistoryRepository;
            _dailyQuizAnswerRepository = dailyQuizAnswerRepository;
            _dailyQuizQuestionRepository = dailyQuizQuestionRepository;
            _dailyQuizWinnerRepository = dailyQuizWinnerRepository;
            _appSetting = appSetting;
            _userService = userService;
            _authContext = authContext;
            _mapper = mapper;
        }

        public async Task<MethodResult<DailyQuizModel>> Handle(DailyQuizCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<DailyQuizModel>();

            var numberQuestion = _appSetting.DailyQuizConfig?.NumberQuestion;
            var numberCorrect = _appSetting.DailyQuizConfig?.NumberCorrect;
            var startDate = _appSetting.DailyQuizConfig?.StartDate;
            var endDate = _appSetting.DailyQuizConfig?.EndDate;
            var endHour = _appSetting.DailyQuizConfig?.EndHour;

            _authContext.AcceptLanguage = DefaultAcceptLanguage;

            if (!numberQuestion.HasValue || !startDate.HasValue || !endDate.HasValue || !numberCorrect.HasValue || !endHour.HasValue)
            {
                methodResult.AddErrorBadRequest(nameof(EnumDailyQuizErrorCode.MissingEventConfiguration), nameof(EnumDailyQuizErrorCode.MissingEventConfiguration), EnumDailyQuizErrorCode.MissingEventConfiguration.GetDescription());
                return methodResult;
            }

            var currentDate = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam);
            if (currentDate.Date < startDate.Value.Date || currentDate.Date > endDate.Value.Date)
            {
                methodResult.AddErrorBadRequest(nameof(EnumDailyQuizErrorCode.EventExpiredOrNotYetOccurred), nameof(EnumDailyQuizErrorCode.EventExpiredOrNotYetOccurred), EnumDailyQuizErrorCode.EventExpiredOrNotYetOccurred.GetDescription());
                return methodResult;
            }

            if (request.DailyQuizzes == null || !request.DailyQuizzes.Any() || request.DailyQuizzes.Count != numberQuestion)
            {
                methodResult.AddErrorBadRequest(nameof(EnumDailyQuizErrorCode.WrongNumberOfQuestions), nameof(EnumDailyQuizErrorCode.WrongNumberOfQuestions), EnumDailyQuizErrorCode.WrongNumberOfQuestions.GetDescription());
                return methodResult;
            }

            if (currentDate.Hour >= endHour.Value)
            {
                methodResult.AddErrorBadRequest(nameof(EnumDailyQuizErrorCode.TimeIsUp), nameof(EnumDailyQuizErrorCode.TimeIsUp), EnumDailyQuizErrorCode.TimeIsUp.GetDescription());
                return methodResult;
            }

            var isDuplicateQuestion = request.DailyQuizzes.GroupBy(p => p.DailyQuizQuestionId).Any(p => p.Count() > 1);
            if (isDuplicateQuestion)
            {
                methodResult.AddErrorBadRequest(nameof(EnumDailyQuizErrorCode.DuplicateQuestion), nameof(EnumDailyQuizErrorCode.DuplicateQuestion), EnumDailyQuizErrorCode.DuplicateQuestion.GetDescription());
                return methodResult;
            }

            var historiesByUser = await _dailyQuizHistoryRepository.Queryable.Where(p => p.CreatedUserId == _authContext.CurrentUserId).ToListAsync(cancellationToken);

            var minCreatedDate = currentDate.Date.AddHours(-7);

            var historiesInDay = historiesByUser.Where(p => p.CreatedDate >= minCreatedDate).ToList();

            if (historiesInDay != null && historiesInDay.Count > 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumDailyQuizErrorCode.CompletedDailyQuizToday), nameof(EnumDailyQuizErrorCode.CompletedDailyQuizToday), EnumDailyQuizErrorCode.CompletedDailyQuizToday.GetDescription());
                return methodResult;
            }

            var questionIds = request.DailyQuizzes.Select(p => p.DailyQuizQuestionId).ToList();
            var answerIds = request.DailyQuizzes.Select(p => p.DailyQuizQuestionId).ToList();

            var histories = historiesByUser.Where(p => questionIds.Contains(p.DailyQuizQuestionId)).ToList();

            if (histories.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumDailyQuizErrorCode.AnsweredThisQuestion), nameof(EnumDailyQuizErrorCode.AnsweredThisQuestion), EnumDailyQuizErrorCode.AnsweredThisQuestion.GetDescription());
                return methodResult;
            }

            var eventResults = await _userService.GetEventsByUserId(null);
            if (!eventResults.IsSuccessStatusCode)
            {
                methodResult.AddError(eventResults.Error);
                return methodResult;
            }

            var @event = eventResults.Content?.Result?.FirstOrDefault();
            if (@event == null || @event.EventContent == null || @event.EventContent.Actions == null || !@event.EventContent.Actions.Any(p => p == EnumSchoolEventRuleAction.DailyQuiz))
            {
                methodResult.AddErrorBadRequest(nameof(EnumDailyQuizErrorCode.NotPartOfTheEvent), nameof(EnumDailyQuizErrorCode.NotPartOfTheEvent), EnumDailyQuizErrorCode.NotPartOfTheEvent.GetDescription());
                return methodResult;
            }

            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddError(studentResult.Error);
                return methodResult;
            }

            var student = studentResult.Content?.Result;
            if (student == null || !student.SchoolId.HasValue)
            {
                methodResult.AddErrorBadRequest(nameof(EnumDailyQuizErrorCode.NotPartOfTheEvent), nameof(EnumDailyQuizErrorCode.NotPartOfTheEvent), EnumDailyQuizErrorCode.NotPartOfTheEvent.GetDescription());
                return methodResult;
            }

            var questionEntities = await _dailyQuizQuestionRepository.Queryable.WhereBulkContains(questionIds, p => p.Id).ToListAsync(cancellationToken);
            var answerEntities = await _dailyQuizAnswerRepository.Queryable.WhereBulkContains(questionIds, p => p.DailyQuizQuestionId).ToListAsync(cancellationToken);

            var questionModels = new List<DailyQuizQuestionModel>();

            var questionDict = questionEntities.ToDictionary(q => q.Id);
            var answerLookup = answerEntities.ToLookup(a => a.DailyQuizQuestionId);

            int correctCount = 0;

            foreach (var item in request.DailyQuizzes)
            {
                if (!questionDict.TryGetValue(item.DailyQuizQuestionId, out var question))
                {
                    methodResult.AddErrorBadRequest(
                        nameof(EnumDailyQuizErrorCode.QuestionDoesNotExist),
                        nameof(EnumDailyQuizErrorCode.QuestionDoesNotExist),
                        EnumDailyQuizErrorCode.QuestionDoesNotExist.GetDescription());
                    return methodResult;
                }

                var answers = answerLookup[item.DailyQuizQuestionId];
                var answer = answers.FirstOrDefault(a => a.Id == item.DailyQuizAnswerId);

                if (answer == null)
                {
                    methodResult.AddErrorBadRequest(
                        nameof(EnumDailyQuizErrorCode.AnswerDoesNotExist),
                        nameof(EnumDailyQuizErrorCode.AnswerDoesNotExist),
                        EnumDailyQuizErrorCode.AnswerDoesNotExist.GetDescription());
                    return methodResult;
                }

                if (answer.IsCorrect)
                {
                    correctCount++;
                }

                var answerModels = _mapper.Map<IList<DailyQuizAnswerModel>>(answers);
                foreach (var ansModel in answerModels)
                {
                    if (ansModel.Id == answer.Id)
                    {
                        ansModel.IsChoice = true;
                        break;
                    }
                }

                var questionModel = _mapper.Map<DailyQuizQuestionModel>(question);
                questionModel.DailyQuizAnswers = answerModels.OrderBy(p => p.Content).ToList();
                questionModels.Add(questionModel);
            }

            string? code = null;

            if (correctCount >= numberCorrect)
            {
                var tickets = await _dailyQuizWinnerRepository.Queryable.Where(p => !string.IsNullOrEmpty(p.Code)).Select(p => p.Code).ToListAsync(cancellationToken);
                do
                {
                    code = Shared.Helpers.NumberHelper.GenerateCodeNumber(6);
                } while (tickets.Contains(code));
            }

            var dailyQuizHistories = _mapper.Map<IList<DailyQuizHistory>>(request.DailyQuizzes);
            int index = 1;
            foreach (var item in dailyQuizHistories)
            {
                item.Index = index;
                index++;
            }

            await _dailyQuizHistoryRepository.ExecuteTransactionAsync(async () =>
            {
                if (!string.IsNullOrEmpty(code))
                {
                    var dailyQuizWinners = new List<DailyQuizWinner>();

                    var dailyQuizWinner = new DailyQuizWinner()
                    {
                        Code = code,
                        SchoolId = student.SchoolId.Value,
                        CompetitionEventId = @event.Id,
                        IsWin = false
                    };

                    if (!dailyQuizWinner.IsValid())
                    {
                        methodResult.AddError(dailyQuizWinner.ErrorMessages);
                        return methodResult;
                    }

                    dailyQuizWinners.Add(dailyQuizWinner);

                    await _dailyQuizWinnerRepository.BulkMergeAsync(dailyQuizWinners, x =>
                    {
                        x.ColumnPrimaryKeyExpression = c => new { c.CreatedUserId, c.CreatedDateLocal };
                    });
                }
                else
                {
                    var deleteWinners = await _dailyQuizWinnerRepository.Queryable.Where(p => p.CreatedUserId == _authContext.CurrentUserId).ToListAsync(cancellationToken);
                    deleteWinners = deleteWinners.Where(p => p.CreatedDateLocal.HasValue && p.CreatedDateLocal.Value.Date == currentDate.Date).ToList();
                    if (deleteWinners.Any())
                    {
                        await _dailyQuizWinnerRepository.DeleteListAsync(deleteWinners);
                    }
                }

                await _dailyQuizHistoryRepository.BulkMergeAsync(dailyQuizHistories, x =>
                {
                    x.ColumnPrimaryKeyExpression = c => new { c.CreatedUserId, c.Index, c.CreatedDateLocal };
                });

                await _dailyQuizHistoryRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.StatusCode = StatusCodes.Status201Created;
                return methodResult;
            });

            var result = new DailyQuizModel()
            {
                Code = code,
                NumberQuestion = numberQuestion.Value,
                NumberCorrect = correctCount,
                DailyQuizQuestions = questionModels.OrderBy(p => p.Content).ToList(),
                IsDone = true
            };

            methodResult.Result = result;
            return methodResult;
        }
    }
}
