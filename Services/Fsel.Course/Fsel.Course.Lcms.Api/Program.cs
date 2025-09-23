// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Extensions;
using Fsel.Course.Application.Services.SystemServices;
using Fsel.Course.Application.Services.UserServices;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Infrastructure;
using Fsel.Course.Infrastructure.Common;
using Fsel.Course.Infrastructure.Repositories;
using Fsel.Course.Infrastructure.ValueSettings;

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
builder.Services.AddScoped<IUnitResultRepository, UnitResultRepository>();
builder.Services.AddScoped<IVideoTimeCodeRepository, VideoTimeCodeRepository>();
builder.Services.AddScoped<IVideoRepository, VideoRepository>();
builder.Services.AddScoped<IVideoTimeCodeAnswerRepository, VideoTimeCodeAnswerRepository>();
builder.Services.AddScoped<ILessonResultRepository, LessonResultRepository>();
builder.Services.AddScoped<IVideoResultRepository, VideoResultRepository>();
builder.Services.AddScoped<IExtraPracticeRepository, ExtraPracticeRepository>();
builder.Services.AddScoped<IExtraPracticeExerciseRepository, ExtraPracticeExerciseRepository>();
builder.Services.AddScoped<IExtraPracticeExerciseResultRepository, ExtraPracticeExerciseResultRepository>();
builder.Services.AddScoped<IExtraPracticeResultRepository, ExtraPracticeResultRepository>();
builder.Services.AddScoped<IExtraPracticeChapterRepository, ExtraPracticeChapterRepository>();

builder.Services.AddScoped<IClassForumRepository, ClassForumRepository>();
builder.Services.AddScoped<IHomeWorkRepository, HomeWorkRepository>();
builder.Services.AddScoped<IExerciseRepository, ExerciseRepository>();
builder.Services.AddScoped<IExerciseQuestionRepository, ExerciseQuestionRepository>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<ICourseTeacherRepository, CourseTeacherRepository>();
builder.Services.AddScoped<ICourseUnitMockTestRepository, CourseUnitMockTestRepository>();
builder.Services.AddScoped<IMockTestRepository, MockTestRepository>();
builder.Services.AddScoped<IMockTestResultRepository, MockTestResultRepository>();
builder.Services.AddScoped<IMockTestAnswerRepository, MockTestAnswerRepository>();
builder.Services.AddScoped<ICourseResultRepository, CourseResultRepository>();
builder.Services.AddScoped<IQuestionFormRepository, QuestionFormRepository>();
builder.Services.AddScoped<ILessonInstructionRepository, LessonInstructionRepository>();
builder.Services.AddScoped<IHomeWorkAnswerRepository, HomeWorkAnswerRepository>();
builder.Services.AddScoped<IHomeWorkQuestionRepository, HomeWorkQuestionRepository>();
builder.Services.AddScoped<IHomeWorkResultRepository, HomeWorkResultRepository>();
builder.Services.AddScoped<IProsodyScoreRepository, ProsodyScoreRepository>();

builder.Services.AddScoped<IPlacementTestGroupResultRepository, PlacementTestGroupResultRepository>();
builder.Services.AddScoped<IPlacementTestResultRepository, PlacementTestResultRepository>();
builder.Services.AddScoped<IPlacementTestAnswerRepository, PlacementTestAnswerRepository>();
builder.Services.AddScoped<IPlacementTestRepository, PlacementTestRepository>();

builder.Services.AddScoped<ISectionGroupRepository, SectionGroupRepository>();
builder.Services.AddScoped<ISectionPartRepository, SectionPartRepository>();
builder.Services.AddScoped<ISectionRepository, SectionRepository>();
builder.Services.AddScoped<ISectionTimeCodeRepository, SectionTimeCodeRepository>();
builder.Services.AddScoped<ISectionQuestionRepository, SectionQuestionRepository>();
builder.Services.AddScoped<ISectionGroupResultRepository, SectionGroupResultRepository>();
builder.Services.AddScoped<IFinalTestRepository, FinalTestRepository>();
builder.Services.AddScoped<IFinalTestAnswerRepository, FinalTestAnswerRepository>();
builder.Services.AddScoped<IFinalTestResultRepository, FinalTestResultRepository>();
builder.Services.AddScoped<IClassForumResultRepository, ClassForumResultRepository>();
builder.Services.AddScoped<IVideoTimeCodeResultRepository, VideoTimeCodeResultRepository>();
builder.Services.AddScoped<IMockTestAISettingRepository, MockTestAISettingRepository>();
builder.Services.AddScoped<IStudentFeedbackRepository, StudentFeedbackRepository>();
builder.Services.AddScoped<IQuestionShuffleRepository, QuestionShuffleRepository>();
builder.Services.AddScoped<IQuestionExplanationLogRepository, QuestionExplanationLogRepository>();
builder.Services.AddScoped<IQuestionExplanationErrorRepository, QuestionExplanationErrorRepository>();
builder.Services.AddScoped<IFinalTestSectionRepository, FinalTestSectionRepository>();
builder.Services.AddScoped<IClassforumDetailResultHistoryRepository, ClassforumDetailResultHistoryRepository>();
builder.Services.AddScoped<ICurriculumStudentRepository, CurriculumStudentRepository>();
builder.Services.AddScoped<ICurriculumRepository, CurriculumRepository>();
builder.Services.AddScoped<IHomeWorkConfigRepository, HomeWorkConfigRepository>();
builder.Services.AddScoped<IHomeWorkExtraPracticeAnswerRepository, HomeWorkExtraPracticeAnswerRepository>();
builder.Services.AddScoped<IHomeWorkExtraPracticeResultRepository, HomeWorkExtraPracticeResultRepository>();
builder.Services.AddScoped<IHomeWorkRetryRepository, HomeWorkRetryRepository>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<ITopicRepository, TopicRepository>();

builder.Services.AddScoped<QuestionTypeConverter>();
builder.Services.AddScoped<ExtraPracticeConverter>();
builder.Services.AddScoped<AnswerTypeConverter>();
builder.Services.AddScoped<VideoConverter>();
builder.Services.AddScoped<CourseHelper>();
builder.Services.AddScoped<UnitHelper>();
builder.Services.AddScoped<QuestionConverter>();
builder.Services.AddScoped<DateTimeConverter>();
builder.Services.AddScoped<SectionGroupConverter>();
builder.Services.AddScoped<SectionGroupManagerConverter>();

// Helper
builder.Services.AddScoped<LinQHelper>();
builder.Services.AddScoped<LinQAnswerHelper>();

builder.AddRefitClients(typeof(IUserService), appSetting?.Services?.UserApiUrl);
builder.AddRefitClients(typeof(ISystemService), appSetting?.Services?.SystemApiUrl);

var app = builder.Build();

app.UseServices();
app.Run();