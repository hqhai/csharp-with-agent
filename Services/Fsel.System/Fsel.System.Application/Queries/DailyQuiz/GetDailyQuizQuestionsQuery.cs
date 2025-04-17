namespace Fsel.System.Application.Queries.DailyQuiz
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.System.Application.Services.UserServices;
    using Fsel.System.Domain.Enums;
    using Fsel.System.Domain.IRepositories.DailyQuizs;
    using Fsel.System.Domain.Models.EntityModels;
    using Fsel.System.Infrastructure.ValueSettings;
    using global::System.Linq;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetDailyQuizQuestionsQuery : IRequest<MethodResult<IList<DailyQuizQuestionModel>>>
    {
    }

    public class GetDailyQuizQuestionsQueryHandler : IRequestHandler<GetDailyQuizQuestionsQuery, MethodResult<IList<DailyQuizQuestionModel>>>
    {
        private readonly IDailyQuizQuestionRepository _dailyQuizQuestionRepository;
        private readonly IDailyQuizAnswerRepository _dailyQuizAnswerRepository;
        private readonly IDailyQuizHistoryRepository _dailyQuizHistoryRepository;
        private readonly IMapper _mapper;
        private readonly AuthContext _authContext;
        private readonly AppSetting _appSetting;
        private readonly IUserService _userService;

        public GetDailyQuizQuestionsQueryHandler(IDailyQuizQuestionRepository dailyQuizQuestionRepository, IDailyQuizAnswerRepository dailyQuizAnswerRepository, IDailyQuizHistoryRepository dailyQuizHistoryRepository, IMapper mapper, AuthContext authContext, AppSetting appSetting, IUserService userService)
        {
            _dailyQuizQuestionRepository = dailyQuizQuestionRepository;
            _dailyQuizAnswerRepository = dailyQuizAnswerRepository;
            _dailyQuizHistoryRepository = dailyQuizHistoryRepository;
            _mapper = mapper;
            _authContext = authContext;
            _appSetting = appSetting;
            _userService = userService;
        }

        public async Task<MethodResult<IList<DailyQuizQuestionModel>>> Handle(GetDailyQuizQuestionsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<DailyQuizQuestionModel>>();

            var numberQuestion = _appSetting.DailyQuizConfig?.NumberQuestion;
            var startDate = _appSetting.DailyQuizConfig?.StartDate;
            var endDate = _appSetting.DailyQuizConfig?.EndDate;
            var endHour = _appSetting.DailyQuizConfig?.EndHour;

            if (!numberQuestion.HasValue || !startDate.HasValue || !endDate.HasValue || !endHour.HasValue)
            {
                methodResult.AddErrorBadRequest(nameof(EnumDailyQuizErrorCode.MissingEventConfiguration), nameof(EnumDailyQuizErrorCode.MissingEventConfiguration), EnumDailyQuizErrorCode.MissingEventConfiguration.GetDescription());
                return methodResult;
            }

            var currentDate = DateTime.UtcNow.ConvertTimeFromUtc(EnumCountryKey.Vietnam);
            if (currentDate.Date < startDate || currentDate > endDate)
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
            if (@event == null)
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

            if (histories.Any(p => p.CreatedDate.Date == currentDate.Date))
            {
                methodResult.AddErrorBadRequest(nameof(EnumDailyQuizErrorCode.CompletedDailyQuizToday), nameof(EnumDailyQuizErrorCode.CompletedDailyQuizToday), EnumDailyQuizErrorCode.CompletedDailyQuizToday.GetDescription());
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
                p.DailyQuizAnswers = answerModels.Where(x => x.DailyQuizQuestionId == p.Id).ToList();
            });

            methodResult.Result = questionModels;
            return methodResult;
        }
    }
}
