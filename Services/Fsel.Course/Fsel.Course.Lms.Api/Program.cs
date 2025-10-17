// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.Constants;
using Fsel.Core.Extensions;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Infrastructure;
using Fsel.Course.Infrastructure.Common;
using Fsel.Course.Infrastructure.Repositories;
using Fsel.Course.Infrastructure.ValueSettings;
using Fsel.Course.Lms.Application.InternalEvents;
using Fsel.Course.Lms.Application.Queues.Consumers;
using Fsel.Course.Lms.Application.Queues.Consumers.ExportFiles;
using Fsel.Course.Lms.Application.Queues.Publishers;
using Fsel.Course.Lms.Application.Queues.Publishers.ExportFiles;
using Fsel.Course.Lms.Application.Services.AiService;
using Fsel.Course.Lms.Application.Services.AiService.SpeakingAIService;
using Fsel.Course.Lms.Application.Services.AIService.SpeakingAIService;
using Fsel.Course.Lms.Application.Services.AIService.SpeakingAIService.Interface;
using Fsel.Course.Lms.Application.Services.FFmpegServices;
using Fsel.Course.Lms.Application.Services.InteractionService;
using Fsel.Course.Lms.Application.Services.NotificationServices;
using Fsel.Course.Lms.Application.Services.OrderServices;
using Fsel.Course.Lms.Application.Services.SenderService;
using Fsel.Course.Lms.Application.Services.StorageServices;
using Fsel.Course.Lms.Application.Services.SystemService;
using Fsel.Course.Lms.Application.Services.TrainingServices;
using Fsel.Course.Lms.Application.Services.UserServices;
using Fsel.Shared.Constants;
using Refit;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var appSetting = builder.AddAppSettings<AppSetting>();
builder.AddServices(appSetting);
builder.AddSwaggerGens(appSetting);
builder.AddAuthenticationJwtBearers(appSetting);
builder.AddDbContexts<CourseDbContext>();

builder.Services.AddScoped<IPlacementTestRepository, PlacementTestRepository>();
builder.Services.AddScoped<ILessonRepository, LessonRepository>();
builder.Services.AddScoped<ILessonVideoRepository, LessonVideoRepository>();
builder.Services.AddScoped<ILessonHomeWorkRepository, LessonHomeWorkRepository>();
builder.Services.AddScoped<ILessonExtraPracticeRepository, LessonExtraPracticeRepository>();
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
builder.Services.AddScoped<ITimeCodeExerciseRepository, TimeCodeExerciseRepository>();
builder.Services.AddScoped<IUnitRepository, UnitRepository>();
builder.Services.AddScoped<IUnitLessonRepository, UnitLessonRepository>();
builder.Services.AddScoped<IVideoTimeCodeRepository, VideoTimeCodeRepository>();
builder.Services.AddScoped<IVideoRepository, VideoRepository>();
builder.Services.AddScoped<IVideoTimeCodeAnswerRepository, VideoTimeCodeAnswerRepository>();
builder.Services.AddScoped<IVideoResultRepository, VideoResultRepository>();
builder.Services.AddScoped<IExtraPracticeRepository, ExtraPracticeRepository>();
builder.Services.AddScoped<IExtraPracticeExerciseRepository, ExtraPracticeExerciseRepository>();
builder.Services.AddScoped<IExtraPracticeExerciseResultRepository, ExtraPracticeExerciseResultRepository>();
builder.Services.AddScoped<IExtraPracticeResultRepository, ExtraPracticeResultRepository>();
builder.Services.AddScoped<IExtraPracticeAnswerRepository, ExtraPracticeAnswerRepository>();
builder.Services.AddScoped<IExtraPracticeChapterRepository, ExtraPracticeChapterRepository>();
builder.Services.AddScoped<BaseInternalUnitResultEventHandler>();
builder.Services.AddScoped<BaseInternalEventHandler>();
builder.Services.AddScoped<MockTestResultInputThenUpdateUnitResultHandler>();

builder.Services.AddScoped<IClassForumRepository, ClassForumRepository>();
builder.Services.AddScoped<IHomeWorkRepository, HomeWorkRepository>();
builder.Services.AddScoped<IExerciseRepository, ExerciseRepository>();
builder.Services.AddScoped<IExerciseQuestionRepository, ExerciseQuestionRepository>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<ICourseTeacherRepository, CourseTeacherRepository>();
builder.Services.AddScoped<ICourseUnitMockTestRepository, CourseUnitMockTestRepository>();
builder.Services.AddScoped<IMockTestRepository, MockTestRepository>();
builder.Services.AddScoped<ILessonRepository, LessonRepository>();
builder.Services.AddScoped<ICourseResultRepository, CourseResultRepository>();
builder.Services.AddScoped<IUnitResultRepository, UnitResultRepository>();
builder.Services.AddScoped<ILessonResultRepository, LessonResultRepository>();
builder.Services.AddScoped<ILessonNoteRepository, LessonNoteRepository>();
builder.Services.AddScoped<IQuestionFormRepository, QuestionFormRepository>();
builder.Services.AddScoped<ILessonInstructionRepository, LessonInstructionRepository>();
builder.Services.AddScoped<IHomeWorkAnswerRepository, HomeWorkAnswerRepository>();
builder.Services.AddScoped<IHomeWorkQuestionRepository, HomeWorkQuestionRepository>();
builder.Services.AddScoped<IHomeWorkResultRepository, HomeWorkResultRepository>();
builder.Services.AddScoped<IMockTestResultRepository, MockTestResultRepository>();
builder.Services.AddScoped<IMockTestAnswerRepository, MockTestAnswerRepository>();
builder.Services.AddScoped<IMockTestSectionRepository, MockTestSectionRepository>();
builder.Services.AddScoped<IUnitSkillMockTestRepository, UnitSkillMockTestRepository>();
builder.Services.AddScoped<ISectionQuestionRepository, SectionQuestionRepository>();
builder.Services.AddScoped<ISectionGroupRepository, SectionGroupRepository>();
builder.Services.AddScoped<ISectionGroupResultRepository, SectionGroupResultRepository>();
builder.Services.AddScoped<ISectionPartRepository, SectionPartRepository>();
builder.Services.AddScoped<ISectionRepository, SectionRepository>();
builder.Services.AddScoped<ISectionTimeCodeRepository, SectionTimeCodeRepository>();
builder.Services.AddScoped<IPlacementTestResultRepository, PlacementTestResultRepository>();
builder.Services.AddScoped<IPlacementTestRepository, PlacementTestRepository>();
builder.Services.AddScoped<IPlacementTestSectionRepository, PlacementTestSectionRepository>();
builder.Services.AddScoped<IPlacementTestAnswerRepository, PlacementTestAnswerRepository>();
builder.Services.AddScoped<ISectionGroupRepository, SectionGroupRepository>();
builder.Services.AddScoped<ISectionPartRepository, SectionPartRepository>();
builder.Services.AddScoped<ISectionRepository, SectionRepository>();
builder.Services.AddScoped<ISectionTimeCodeRepository, SectionTimeCodeRepository>();
builder.Services.AddScoped<ISectionQuestionRepository, SectionQuestionRepository>();
builder.Services.AddScoped<IFinalTestRepository, FinalTestRepository>();
builder.Services.AddScoped<IFinalTestAnswerRepository, FinalTestAnswerRepository>();
builder.Services.AddScoped<IFinalTestResultRepository, FinalTestResultRepository>();
builder.Services.AddScoped<IClassForumFileRepository, ClassForumFileRepository>();
builder.Services.AddScoped<IClassForumResultRepository, ClassForumResultRepository>();
builder.Services.AddScoped<IClassForumResultFileRepository, ClassForumResultFileRepository>();
builder.Services.AddScoped<IClassForumScoreRepository, ClassForumScoreRepository>();
builder.Services.AddScoped<IStudentFeedbackRepository, StudentFeedbackRepository>();
builder.Services.AddScoped<IMockTestScoreRepository, MockTestScoreRepository>();
builder.Services.AddScoped<IClassForumResultRandomRepository, ClassForumResultRandomRepository>();
builder.Services.AddScoped<IVideoTimeCodeResultRepository, VideoTimeCodeResultRepository>();
builder.Services.AddScoped<IMockTestAISettingRepository, MockTestAISettingRepository>();
builder.Services.AddScoped<IClassForumDetailResultRepository, ClassForumDetailResultRepository>();
builder.Services.AddScoped<IProsodyScoreRepository, ProsodyScoreRepository>();
builder.Services.AddScoped<ISpeakingAIService, SpeakingAIService>();
builder.Services.AddScoped<ISpeakingEvaluationAIService, SpeakingEvaluationAIService>();
builder.Services.AddScoped<IQuestionExplanationErrorRepository, QuestionExplanationErrorRepository>();
builder.Services.AddScoped<IQuestionExplanationLogRepository, QuestionExplanationLogRepository>();
builder.Services.AddScoped<IPlacementTestGroupResultRepository, PlacementTestGroupResultRepository>();
builder.Services.AddScoped<IQuestionShuffleRepository, QuestionShuffleRepository>();
builder.Services.AddScoped<IWeeklyReportRepository, WeeklyReportRepository>();
builder.Services.AddScoped<IFinalTestSectionRepository, FinalTestSectionRepository>();
builder.Services.AddScoped<IClassforumDetailResultHistoryRepository, ClassforumDetailResultHistoryRepository>();

builder.Services.AddScoped<QuestBoardPublisher>();
builder.Services.AddScoped<SubmitMockTestAnswerPublisher>();
builder.Services.AddScoped<CreateTokenHistoryPublisher>();
builder.Services.AddScoped<SetTimeRetryMockTestPublisher>();
builder.Services.AddScoped<SetTimeRetryClassForumPublisher>();
builder.Services.AddScoped<TechieActionPublisher>();
builder.Services.AddScoped<CreateLuckyTicketPublisher>();
builder.Services.AddScoped<AddFeatureMissionPublisher>();

// Converter
builder.Services.AddScoped<ExtraPracticeConverter>();
builder.Services.AddScoped<QuestionTypeConverter>();
builder.Services.AddScoped<AnswerTypeConverter>();
builder.Services.AddScoped<VideoConverter>();
builder.Services.AddScoped<CourseHelper>();
builder.Services.AddScoped<UnitHelper>();
builder.Services.AddScoped<QuestionConverter>();
builder.Services.AddScoped<SectionGroupConverter>();
builder.Services.AddScoped<DateTimeConverter>();
builder.Services.AddScoped<SectionGroupManagerConverter>();

// Helper
builder.Services.AddScoped<LinQHelper>();
builder.Services.AddScoped<LinQAnswerHelper>();
builder.Services.AddScoped<ChangeCourseHelper>();
builder.Services.AddScoped<ManagerProgressHelper>();

// Publisher
builder.Services.AddScoped<QuestBoardPublisher>();
builder.Services.AddScoped<SaveUserCourseSettingPublisher>();
builder.Services.AddScoped<CreateTokenHistoryPublisher>();
builder.Services.AddScoped<FinishOneFinalTestPublisher>();
builder.Services.AddScoped<SetTimeClassForumDonePublisher>();
builder.Services.AddScoped<FinishOneHomeWorkPublisher>();
builder.Services.AddScoped<FinishOneLessonPublisher>();
builder.Services.AddScoped<FinishOneLevelPassPublisher>();
builder.Services.AddScoped<FinishOneUnitPublisher>();
builder.Services.AddScoped<FinishOneUnitTestPublisher>();
builder.Services.AddScoped<NotificationMessagePublisher>();
builder.Services.AddScoped<CreateOrderPublisher>();
builder.Services.AddScoped<GetTimeToCompleteTestPublisher>();
builder.Services.AddScoped<SubmitAIResponsePublisher>();
builder.Services.AddScoped<SubmitClassForumGradingPublisher>();
builder.Services.AddScoped<SubmitMockTestAnswerPublisher>();
builder.Services.AddScoped<SubmitMockTestCriteriaPublisher>();
builder.Services.AddScoped<CreateTokenHistoryPublisher>();
builder.Services.AddScoped<DisconnectSocketCalculateTimePublisher>();
builder.Services.AddScoped<SubmitAiSpeakingAnswerPublisher>();
builder.Services.AddScoped<GetTimeModulePublisher>();
builder.Services.AddScoped<SubmitSpeakingAIPublisher>();
builder.Services.AddScoped<StudentRankingEventsPublisher>();
builder.Services.AddScoped<RankedStudentPublisher>();
builder.Services.AddScoped<ExportFileExcelStudentLearningProcessPublisher>();
builder.Services.AddScoped<SavePlacementTestAnswersPublisher>();
builder.Services.AddScoped<ErrorExplainPublisher>();
builder.Services.AddScoped<ExportFileExcelSchoolLearningProcessPublisher>();
builder.Services.AddScoped<SpeechToTextPendingAiPublisher>();
builder.Services.AddScoped<ExportFileUserInformationSupportSalePublisher>();

// Refit
builder.AddRefitClients(typeof(IUserService), appSetting?.Services?.UserApiUrl);
builder.AddRefitClients(typeof(ITrainingService), appSetting?.Services?.ClassApiUrl);
builder.AddRefitClients(typeof(IInteractionService), appSetting?.Services?.InteractionApiUrl);
builder.AddRefitClients(typeof(ISystemService), appSetting?.Services?.SystemApiUrl);
builder.AddRefitClients(typeof(IOrderService), appSetting?.Services?.OrderApiUrl);
builder.AddRefitClients(typeof(ISenderService), appSetting?.Services?.SenderApiUrl);
builder.AddRefitClients(typeof(INotificationService), appSetting?.Services?.NotificationApiUrl);
builder.AddRefitClients(typeof(IStorageService), appSetting?.Services?.StorageApiUrl);
builder.AddRefitClients(typeof(IFFmpegServices), appSetting?.Services?.FFmpegApiUrl);
builder.Services.AddRefitClient<IOpenAIService>().ConfigureHttpClient(delegate (IServiceProvider serviceProvider, HttpClient httpClient)
{
    httpClient.BaseAddress = new Uri(appSetting?.OpenAiConfig?.Uri ?? string.Empty);
    if (appSetting?.OpenAiConfig?.ApiKeys != null && appSetting.OpenAiConfig.ApiKeys!.Any())
    {
        var randomApiKey = appSetting.OpenAiConfig.ApiKeys[Random.Shared.Next(appSetting.OpenAiConfig.ApiKeys.Count)];
        httpClient.DefaultRequestHeaders.Add("Authorization", $"{Settings.Bearer} {randomApiKey}");
    }
});

builder.AddMassTransit(appSetting,
queues: new Dictionary<string, Type>
{
    { QueueSettings.LmsQueue.NameQueue.UpdateOcCheckInClassForumResult, typeof(UpdateOcCheckInClassForumResultConsumer) },
    { QueueSettings.LmsQueue.NameQueue.CompleteTestWhenTimeOut, typeof(CompleteTestWhenTimeOutConsumer) },
    { QueueSettings.LmsQueue.NameQueue.UpdateTeacherGradingInClassForumAndMockTest, typeof(UpdateTeacherGradingInClassForumAndMockTestConsumer) },
    { QueueSettings.LmsQueue.NameQueue.DeleteClassForumByFlag, typeof(DeleteClassForumByFlagConsumer) },
    { QueueSettings.LmsQueue.NameQueue.ClassForumAIResponse, typeof(RealTimeAIResponseConsumer) },
    { QueueSettings.LmsQueue.NameQueue.MockTestAnwserResponse, typeof(AiFeedBackResponseConsumer) },
    { QueueSettings.LmsQueue.NameQueue.UpdateClassForumResultToExpiredTime, typeof(UpdateClassForumResultToExpiredTimeConsumer) },
    { QueueSettings.LmsQueue.NameQueue.SendWeeklyReport, typeof(SendWeeklyReportConsumer) },
    { QueueSettings.LmsQueue.NameQueue.AggregateDataWeeklyReport, typeof(AggregateDataWeeklyReportConsumer) },
    { QueueSettings.RealtimeQueue.NameQueue.SetTimeModule, typeof(SetTimeModuleConsumer) },
    { QueueSettings.LmsQueue.NameQueue.RetryMockTestAction, typeof(RetryMockTestWhenScoreZeroConsumer) },
    { QueueSettings.RealtimeQueue.NameQueue.GetTimeModule, typeof(GetTimeModuleConsumer) },
    { QueueSettings.LmsQueue.NameQueue.RetryClassForumAction, typeof(RetryClassForumConsumer) },
    { QueueSettings.LmsQueue.NameQueue.SpeakingAI, typeof(SpeakingAIEvaluationConsumer) },
    { QueueSettings.LmsQueue.NameQueue.RankedStudent, typeof(RankedStudentConsumer) },
    { QueueSettings.LmsQueue.NameQueue.ExportExcelStudentLearningProcess, typeof(ExportFileExcelStudentLearningProcessConsumer) },
    { QueueSettings.LmsQueue.NameQueue.ExportExcelSchoolLearningProcess, typeof(ExportExcelSchoolLearningProcessConsumer) },
    { QueueSettings.LmsQueue.NameQueue.SavePlacementTestAnswers, typeof(SavePlacementTestAnswersConsumer) },
    { QueueSettings.LmsQueue.NameQueue.ErrorExplainGgSheet, typeof(ErrorExplainConsumer) },
    { QueueSettings.StorageQueue.NameQueue.ResponseSpeechToTextPendingAi, typeof(ResponseSpeechToTextPendingAiConsumer) },
    { QueueSettings.LmsQueue.NameQueue.PushNotice, typeof(PushNoticeConsumer) },
    { QueueSettings.LmsQueue.NameQueue.ExportExcelUserInformationSupportSale, typeof(ExportFileUserInformationSupportSaleConsumer) },
});

var app = builder.Build();
app.UseServices();
app.Run();
