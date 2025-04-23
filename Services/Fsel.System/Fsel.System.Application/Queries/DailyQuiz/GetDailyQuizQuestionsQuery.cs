namespace Fsel.System.Application.Queries.DailyQuiz
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
    using Fsel.System.Domain.Models.EntityModels;
    using Fsel.System.Infrastructure.ValueSettings;
    using global::System.Linq;
    using global::System.Threading;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetDailyQuizQuestionsQuery : IRequest<MethodResult<DailyQuizModel>>
    {
    }

    public class GetDailyQuizQuestionsQueryHandler : IRequestHandler<GetDailyQuizQuestionsQuery, MethodResult<DailyQuizModel>>
    {
        private readonly IDailyQuizQuestionRepository _dailyQuizQuestionRepository;
        private readonly IDailyQuizAnswerRepository _dailyQuizAnswerRepository;
        private readonly IDailyQuizHistoryRepository _dailyQuizHistoryRepository;
        private readonly IDailyQuizWinnerRepository _dailyQuizWinnerRepository;
        private readonly IMapper _mapper;
        private readonly AuthContext _authContext;
        private readonly AppSetting _appSetting;
        private readonly IUserService _userService;
        private const string DefaultAcceptLanguage = "en-US";

        public GetDailyQuizQuestionsQueryHandler(IDailyQuizQuestionRepository dailyQuizQuestionRepository, IDailyQuizAnswerRepository dailyQuizAnswerRepository, IDailyQuizHistoryRepository dailyQuizHistoryRepository, IMapper mapper, AuthContext authContext, AppSetting appSetting, IUserService userService, IDailyQuizWinnerRepository dailyQuizWinnerRepository)
        {
            _dailyQuizQuestionRepository = dailyQuizQuestionRepository;
            _dailyQuizAnswerRepository = dailyQuizAnswerRepository;
            _dailyQuizHistoryRepository = dailyQuizHistoryRepository;
            _mapper = mapper;
            _authContext = authContext;
            _appSetting = appSetting;
            _userService = userService;
            _dailyQuizWinnerRepository = dailyQuizWinnerRepository;
        }

        public async Task<MethodResult<DailyQuizModel>> Handle(GetDailyQuizQuestionsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<DailyQuizModel>();

            var numberQuestion = _appSetting.DailyQuizConfig?.NumberQuestion;
            var startDate = _appSetting.DailyQuizConfig?.StartDate;
            var endDate = _appSetting.DailyQuizConfig?.EndDate;
            var endHour = _appSetting.DailyQuizConfig?.EndHour;

            _authContext.AcceptLanguage = DefaultAcceptLanguage;

            if (!numberQuestion.HasValue || !startDate.HasValue || !endDate.HasValue || !endHour.HasValue)
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

            if (currentDate.Hour >= endHour.Value)
            {
                methodResult.AddErrorBadRequest(nameof(EnumDailyQuizErrorCode.TimeIsUp), nameof(EnumDailyQuizErrorCode.TimeIsUp), EnumDailyQuizErrorCode.TimeIsUp.GetDescription());
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

            var histories = await _dailyQuizHistoryRepository.Queryable.Where(p => p.CreatedUserId == _authContext.CurrentUserId).ToListAsync(cancellationToken);

            var minCreatedDate = currentDate.Date.AddHours(-7);

            var historiesInDay = histories.Where(p => p.CreatedDate >= minCreatedDate).ToList();

            var result = new DailyQuizModel()
            {
                Code = null,
                NumberQuestion = numberQuestion.Value,
                NumberCorrect = 0
            };

            if (historiesInDay != null && historiesInDay.Count > 0)
            {
                methodResult = await GetQuestionHistory(historiesInDay, result, methodResult, minCreatedDate, cancellationToken);
                return methodResult;
            }

            var historyIds = histories.Select(p => p.DailyQuizQuestionId);

            var questions = await _dailyQuizQuestionRepository.Queryable.WhereBulkNotContains(historyIds, p => p.Id).ToListAsync(cancellationToken);

            if (questions.Count < 3)
            {
                methodResult.AddErrorBadRequest(nameof(EnumDailyQuizErrorCode.NumberOfQuestionsOut), nameof(EnumDailyQuizErrorCode.NumberOfQuestionsOut), EnumDailyQuizErrorCode.NumberOfQuestionsOut.GetDescription());
                return methodResult;
            }

            var random = new Random();
            var items = questions.OrderBy(x => random.Next()).Take(numberQuestion.Value).ToList();
            var itemIds = items.Select(p => p.Id).ToList();

            var answers = await _dailyQuizAnswerRepository.Queryable.WhereBulkContains(itemIds, p => p.DailyQuizQuestionId).ToListAsync(cancellationToken);

            var questionModels = _mapper.Map<IList<DailyQuizQuestionModel>>(items);
            var answerModels = _mapper.Map<IList<DailyQuizAnswerModel>>(answers);

            answerModels.ForEach(p => p.IsCorrect = false);

            questionModels.ForEach(p =>
            {
                p.Explanation = null;
                p.DailyQuizAnswers = answerModels.Where(x => x.DailyQuizQuestionId == p.Id).OrderBy(p => p.Content).ToList();
            });

            result.DailyQuizQuestions = questionModels.OrderBy(p => p.Content).ToList();

            methodResult.Result = result;
            return methodResult;
        }

        private async Task<MethodResult<DailyQuizModel>> GetQuestionHistory(IList<DailyQuizHistory> historiesInDay, DailyQuizModel result, MethodResult<DailyQuizModel> methodResult, DateTime currentDate, CancellationToken cancellationToken)
        {
            var questionIds = historiesInDay.Select(p => p.DailyQuizQuestionId).ToList();
            var answerIds = historiesInDay.Select(p => p.DailyQuizQuestionId).ToList();

            var questionEntities = await _dailyQuizQuestionRepository.Queryable.WhereBulkContains(questionIds, p => p.Id).ToListAsync(cancellationToken);
            var answerEntities = await _dailyQuizAnswerRepository.Queryable.WhereBulkContains(questionIds, p => p.DailyQuizQuestionId).ToListAsync(cancellationToken);

            var questionDict = questionEntities.ToDictionary(q => q.Id);
            var answerLookup = answerEntities.ToLookup(a => a.DailyQuizQuestionId);

            int correctCount = 0;

            var dailyQuizQuestionModels = new List<DailyQuizQuestionModel>();

            foreach (var item in historiesInDay)
            {
                if (!questionDict.TryGetValue(item.DailyQuizQuestionId, out var question))
                {
                    methodResult.AddErrorBadRequest(
                        nameof(EnumDailyQuizErrorCode.QuestionDoesNotExist),
                        nameof(EnumDailyQuizErrorCode.QuestionDoesNotExist),
                        EnumDailyQuizErrorCode.QuestionDoesNotExist.GetDescription());
                    return methodResult;
                }

                var dailyQuizAnswers = answerLookup[item.DailyQuizQuestionId];
                var answer = dailyQuizAnswers.FirstOrDefault(a => a.Id == item.DailyQuizAnswerId);

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

                var dailyQuizAnswerModels = _mapper.Map<IList<DailyQuizAnswerModel>>(dailyQuizAnswers);
                foreach (var ansModel in dailyQuizAnswerModels)
                {
                    if (ansModel.Id == answer.Id)
                    {
                        ansModel.IsChoice = true;
                        break;
                    }
                }

                var questionModel = _mapper.Map<DailyQuizQuestionModel>(question);
                questionModel.DailyQuizAnswers = dailyQuizAnswerModels.OrderBy(p => p.Content).ToList();
                dailyQuizQuestionModels.Add(questionModel);
            }

            var winner = await _dailyQuizWinnerRepository.Queryable.FirstOrDefaultAsync(p => p.CreatedDate >= currentDate && p.CreatedUserId == _authContext.CurrentUserId, cancellationToken);

            result.Code = winner?.Code;
            result.IsDone = true;
            result.NumberCorrect = correctCount;
            result.DailyQuizQuestions = dailyQuizQuestionModels.OrderBy(p => p.Content).ToList();

            methodResult.Result = result;
            return methodResult;
        }
    }
}
