// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.Constants;
using Fsel.Core.Extensions;
using Fsel.Core.Middlewares;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Infrastructure;
using Fsel.Course.Infrastructure.Common;
using Fsel.Course.Infrastructure.Common.LessonHelpers;
using Fsel.Course.Infrastructure.Common.QuestionHelper.QuestionTypes;
using Fsel.Course.Infrastructure.Repositories;
using Fsel.Course.Infrastructure.ValueSettings;
using Fsel.Course.Lms.Application.InternalEvents;
using Fsel.Course.Lms.Application.InternalEvents.BaseCourseModule;
using Fsel.Course.Lms.Application.InternalEvents.BaseUnitModule;
using Fsel.Course.Lms.Application.Queues.Consumers;
using Fsel.Course.Lms.Application.Queues.Consumers.ExportFiles;
using Fsel.Course.Lms.Application.Queues.Consumers.Test;
using Fsel.Course.Lms.Application.Queues.Publishers;
using Fsel.Course.Lms.Application.Queues.Publishers.ExportFiles;
using Fsel.Course.Lms.Application.Queues.Publishers.Test;
using Fsel.Course.Lms.Application.Services.AIConfigService;
using Fsel.Course.Lms.Application.Services.AiService;
using Fsel.Course.Lms.Application.Services.AiService.SpeakingAIService;
using Fsel.Course.Lms.Application.Services.AIService.SpeakingAIService;
using Fsel.Course.Lms.Application.Services.AIService.SpeakingAIService.Interface;
using Fsel.Course.Lms.Application.Services.ApplicationServices;
using Fsel.Course.Lms.Application.Services.ApplicationServices.CacheServices;
using Fsel.Course.Lms.Application.Services.ApplicationServices.CacheServices.BuildModules;
using Fsel.Course.Lms.Application.Services.ApplicationServices.ChangeCourse;
using Fsel.Course.Lms.Application.Services.ApplicationServices.LearningServices.CourseItemServices;
using Fsel.Course.Lms.Application.Services.ApplicationServices.LearningServices.LessonItemServices;
using Fsel.Course.Lms.Application.Services.ApplicationServices.LearningServices.UnitItemServices;
using Fsel.Course.Lms.Application.Services.FFmpegServices;
using Fsel.Course.Lms.Application.Services.InteractionService;
using Fsel.Course.Lms.Application.Services.NotificationServices;
using Fsel.Course.Lms.Application.Services.OrderServices;
using Fsel.Course.Lms.Application.Services.SenderService;
using Fsel.Course.Lms.Application.Services.StorageServices;
using Fsel.Course.Lms.Application.Services.SystemService;
using Fsel.Course.Lms.Application.Services.TestServices;
using Fsel.Course.Lms.Application.Services.TestServices.Interface;
using Fsel.Course.Lms.Application.Services.TrainingServices;
using Fsel.Course.Lms.Application.Services.UserServices;
using Fsel.Shared.Constants;
using Refit;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var appSetting = builder.AddAppSettings<AppSetting>();
builder.AddServices(appSetting);
builder.AddOpenIdSwaggerGens(appSetting);
builder.AddOpenIdAuthenticationJwtBearers(appSetting);
builder.AddDbContexts<CourseDbContext, CourseReadDbContext>();

// HttpClient
builder.Services.AddHttpClient();

builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IFlowService, FlowService>();
builder.Services.AddScoped<ITestService, TestService>();
builder.Services.AddScoped<ITestCachingService, TestCachingService>();
builder.Services.AddScoped<IFlowCachingService, FlowCachingService>();
builder.Services.AddScoped<ICategoryCachingService, CategoryCachingService>();
builder.Services.AddScoped<IDocumentCachingService, DocumentCachingService>();
builder.Services.AddScoped<ICourseCachingService, CourseCachingService>();
builder.Services.AddScoped<IUnitModuleCachingService, UnitModuleCachingService>();
builder.Services.AddScoped<ICourseModuleCachingService, CourseModuleCachingService>();
builder.Services.AddScoped<ILessonModuleCachingService, LessonModuleCachingService>();
builder.Services.AddScoped<IAggregateResultQueryService, AggregateResultQueryService>();
builder.Services.AddScoped<IChangeCourseService, ChangeCourseService>();

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
builder.Services.AddScoped<IPronuciationAssessmentService, PronuciationAssessmentService>();
builder.Services.AddScoped<IAIConfigSubmitService, AIConfigSubmitService>();
builder.Services.AddScoped<IContinuousPronunciationAssessmentService, PronuciationAssessmentService>();
builder.Services.AddScoped<IQuestionExplanationErrorRepository, QuestionExplanationErrorRepository>();
builder.Services.AddScoped<IQuestionExplanationLogRepository, QuestionExplanationLogRepository>();
builder.Services.AddScoped<IPlacementTestGroupResultRepository, PlacementTestGroupResultRepository>();
builder.Services.AddScoped<IQuestionShuffleRepository, QuestionShuffleRepository>();
builder.Services.AddScoped<IWeeklyReportRepository, WeeklyReportRepository>();
builder.Services.AddScoped<IFinalTestSectionRepository, FinalTestSectionRepository>();
builder.Services.AddScoped<IClassforumDetailResultHistoryRepository, ClassforumDetailResultHistoryRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ILevelRepository, LevelRepository>();
builder.Services.AddScoped<ISkillRepository, SkillRepository>();
builder.Services.AddScoped<ISkillLevelRepository, SkillLevelRepository>();
builder.Services.AddScoped<IFlowRepository, FlowRepository>();
builder.Services.AddScoped<IStepFlowRepository, StepFlowRepository>();
builder.Services.AddScoped<IActionFlowRepository, ActionFlowRepository>();
builder.Services.AddScoped<ILessonModuleRepository, LessonModuleRepository>();
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<ITestRepository, TestRepository>();
builder.Services.AddScoped<ITestSectionRepository, TestSectionRepository>();
builder.Services.AddScoped<ITestSectionQuestionRepository, TestConfigSectionQuestionRepository>();
builder.Services.AddScoped<ITestAISettingRepository, TestAISettingRepository>();
builder.Services.AddScoped<ITestAICriteriaSettingRepository, TestAICriteriaSettingRepository>();
builder.Services.AddScoped<ISubjectConditionRepository, SubjectConditionRepository>();
builder.Services.AddScoped<ISubjectConditionRuleRepository, SubjectConditionRuleRepository>();
builder.Services.AddScoped<IKeyboardTextRepository, KeyboardTextRepository>();
builder.Services.AddScoped<IKeyboardLayoutRepository, KeyboardLayoutRepository>();
builder.Services.AddScoped<ICategoryTestBankRepository, CategoryTestBankRepository>();
builder.Services.AddScoped<ITopicRepository, TopicRepository>();
builder.Services.AddScoped<ICurriculumStudentRepository, CurriculumStudentRepository>();
builder.Services.AddScoped<ICurriculumRepository, CurriculumRepository>();
builder.Services.AddScoped<IHomeWorkConfigRepository, HomeWorkConfigRepository>();
builder.Services.AddScoped<IHomeWorkExtraPracticeAnswerRepository, HomeWorkExtraPracticeAnswerRepository>();
builder.Services.AddScoped<IHomeWorkExtraPracticeResultRepository, HomeWorkExtraPracticeResultRepository>();
builder.Services.AddScoped<IHomeWorkRetryRepository, HomeWorkRetryRepository>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IVideoSubFilePathRepository, VideoSubFilePathRepository>();
builder.Services.AddScoped<IStudentGoalAggregateRepository, StudentGoalAggregateRepository>();
builder.Services.AddScoped<IStudentGoalSummaryRepository, StudentGoalSummaryRepository>();
builder.Services.AddScoped<IStatusStudentGoalRepository, StatusStudentGoalRepository>();
builder.Services.AddScoped<IAiPromptManagerRepository, AiPromptManagerRepository>();
builder.Services.AddScoped<IAiCriteriaConfigRepository, AiFeatureConfigRepository>();
builder.Services.AddScoped<ITestSectionResultRepository, TestSectionResultRepository>();

builder.Services.AddScoped<ISpeakingAITestLayoutHandler, SpeakingAITestLayoutHandler>();
builder.Services.AddScoped<IWritingAITestLayoutHandler, WritingAITestLayoutHandler>();
builder.Services.AddScoped<ITestAiLayoutService, TestAiLayoutService>();

builder.Services.AddScoped<IDocumentResultRepository, DocumentResultRepository>();
builder.Services.AddScoped<ITestResultRepository, TestResultRepository>();
builder.Services.AddScoped<ITestGroupResultRepository, TestGroupResultRepository>();
builder.Services.AddScoped<IUnitModuleRepository, UnitModuleRepository>();
builder.Services.AddScoped<ICourseModuleRepository, CourseModuleRepository>();
builder.Services.AddScoped<ICourseChangingHistoryRepository, CourseChangingHistoryRepository>();

builder.Services.AddScoped<VideoLessonItemInitializer>();
builder.Services.AddScoped<ClassForumLessonItemInitializer>();
builder.Services.AddScoped<HomeWorkLessonItemInitializer>();
builder.Services.AddScoped<DocumentLessonItemInitializer>();
builder.Services.AddScoped<ILessonItemInitializerFactory, LessonItemInitializerFactory>();
builder.Services.AddScoped<TestUnitItemInitializer>();
builder.Services.AddScoped<LessonUnitItemInitializer>();
builder.Services.AddScoped<IUnitItemInitializerFactory, UnitItemInitializerFactory>();
builder.Services.AddScoped<TestCourseItemInitializer>();
builder.Services.AddScoped<UnitCourseItemInitializer>();
builder.Services.AddScoped<ICourseItemInitializerFactory, CourseItemInitializerFactory>();
builder.Services.AddScoped<ICourseResultUpdater, BaseCourseResultEventHandler>();
builder.Services.AddScoped<IUnitResultUpdater, BaseUnitResultEventHandler>();

builder.Services.AddScoped<IVideoTimeCodeModelCachingService, VideoTimeCodeModelCachingService>();
builder.Services.AddScoped<ITimeCodeQuestionCachingService, TimeCodeQuestionCachingService>();
builder.Services.AddScoped<ITestSectionCachingService, TestSectionCachingService>();
builder.Services.AddScoped<ISpeakingAITestLayoutHandler, SpeakingAITestLayoutHandler>();
builder.Services.AddScoped<IWritingAITestLayoutHandler, WritingAITestLayoutHandler>();
builder.Services.AddScoped<IProgramSkillScoresCachingService, ProgramSkillScoresCachingService>();
builder.Services.AddScoped<ICourseBuildCachingService, CourseBuildCachingService>();

builder.Services.AddScoped<IProgressService, ProgressService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IVideoService, VideoService>();
builder.Services.AddScoped<IVideoCachingService, VideoCachingService>();
builder.Services.AddScoped<IVideoTimeCodeService, VideoTimeCodeService>();
builder.Services.AddScoped<ILearningGoalAggregateService, LearningGoalAggregateService>();
builder.Services.AddScoped<IStudentGoalProgressService, StudentGoalProgressService>();
builder.Services.AddScoped<ILearningService, LearningService>();

builder.Services.AddScoped<QuestBoardPublisher>();
builder.Services.AddScoped<SubmitMockTestAnswerPublisher>();
builder.Services.AddScoped<CreateTokenHistoryPublisher>();
builder.Services.AddScoped<SetTimeRetryMockTestPublisher>();
builder.Services.AddScoped<SetTimeRetryClassForumPublisher>();
builder.Services.AddScoped<TechieActionPublisher>();
builder.Services.AddScoped<CreateLuckyTicketPublisher>();
builder.Services.AddScoped<AddFeatureMissionPublisher>();
builder.Services.AddScoped<SaveUserSurveyAssignmentPublisher>();
builder.Services.AddScoped<SubmitAiTestLayOutPublisher>();

// Converter
builder.Services.AddScoped<ExtraPracticeConverter>();
builder.Services.AddScoped<QuestionTypeConverter>();
builder.Services.AddScoped<AnswerTypeConverter>();
builder.Services.AddScoped<VideoConverter>();
builder.Services.AddScoped<CourseHelper>();
builder.Services.AddScoped<QuestionConverter>();
builder.Services.AddScoped<SectionGroupConverter>();
builder.Services.AddScoped<DateTimeConverter>();
builder.Services.AddScoped<SectionGroupManagerConverter>();
builder.Services.AddScoped<ProgramConverter>();
builder.Services.AddScoped<LessonConverter>();
builder.Services.AddScoped<TestConverter>();

// Helper
builder.Services.AddScoped<LinQAnswerHelper>();
builder.Services.AddScoped<ChangeCourseHelper>();
builder.Services.AddScoped<ManagerProgressHelper>();
builder.Services.AddScoped<SubjectConditionHelper>();
builder.Services.AddScoped<QuestionTypeFactory>();

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
builder.Services.AddScoped<ClassForumTranslationPublisher>();
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
builder.Services.AddScoped<ClassForumPronunciationPublisher>();
builder.Services.AddScoped<ExportFileUserInformationSupportSalePublisher>();
builder.Services.AddScoped<SubmitTestAiSpeakingPublisher>();
builder.Services.AddScoped<SubmitTestCriteriaPublisher>();
builder.Services.AddScoped<SetTimeRetryTestPublisher>();
builder.Services.AddScoped<TranslationResultPublisher>();
builder.Services.AddScoped<SendMailFinishCoursePublisher>();
builder.Services.AddScoped<SendMailFinishPTPublisher>();
builder.Services.AddScoped<SendMailCompleteUnitPublisher>();

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
    { QueueSettings.LmsQueue.NameQueue.ClassForumTranslationRequest, typeof(ClassForumTranslationConsumer) },
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
    { QueueSettings.LmsQueue.NameQueue.ClassForumPronunciationAi, typeof(ClassForumPronunciationConsumer) },
    { QueueSettings.StorageQueue.NameQueue.ResponseSpeechToTextPendingAi, typeof(ResponseSpeechToTextPendingAiConsumer) },
    { QueueSettings.LmsQueue.NameQueue.PushNotice, typeof(PushNoticeConsumer) },
    { QueueSettings.RealtimeQueue.NameQueue.QuestionType, typeof(QuestionTypeConsumer) },
    { QueueSettings.LmsQueue.NameQueue.ExportExcelUserInformationSupportSale, typeof(ExportFileUserInformationSupportSaleConsumer) },
    { QueueSettings.LmsQueue.NameQueue.JobStudentAggregate, typeof(JobStudentAggregateConsumer) },
    { QueueSettings.LmsQueue.NameQueue.NotifyWeeklyReportCourseTarget, typeof(NotifyWeeklyReportCourseTargetConsumer) },
    { QueueSettings.LmsQueue.NameQueue.NotifyWeeklyCourseGoalTarget, typeof(NotifyWeeklyCourseGoalTargetConsumer) },
    { QueueSettings.LmsQueue.NameQueue.SubmitTestAi, typeof(SubmitAiTestLayOutConsumer) },
    { QueueSettings.LmsQueue.NameQueue.AITranslationResponse, typeof(AITranslationResponseConsumer) },
    { QueueSettings.LmsQueue.NameQueue.SendMailFinishCourse, typeof(SendMailFinishCourseConsumer) },
    { QueueSettings.LmsQueue.NameQueue.SendMailFinishPT, typeof(SendMailFinishPTConsumer) },
    { QueueSettings.LmsQueue.NameQueue.SendMailCompleteUnit, typeof(SendMailCompleteUnitConsumer) },
});

var app = builder.Build();
app.UseMiddleware<CacheManagerMiddleware>("/cache");

app.UseServices();
app.Run();
