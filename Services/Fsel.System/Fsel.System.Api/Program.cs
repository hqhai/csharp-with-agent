// Copyright (c) Atlantic. All rights reserved.

using Amazon.Runtime.Internal.Transform;
using Fsel.Common.Constants;
using Fsel.Core.Extensions;
using Fsel.Shared.Constants;
using Fsel.System.Application.Queues.Consumers;
using Fsel.System.Application.Queues.Publisher;
using Fsel.System.Application.Services.AIServices;
using Fsel.System.Application.Services.CourseServices;
using Fsel.System.Application.Services.DictionaryServices;
using Fsel.System.Application.Services.FFmpegServices;
using Fsel.System.Application.Services.GoogleSheetServices;
using Fsel.System.Application.Services.OrderServices;
using Fsel.System.Application.Services.SenderServices;
using Fsel.System.Application.Services.StorageServices;
using Fsel.System.Application.Services.UserServices;
using Fsel.System.Domain.IRepositories;
using Fsel.System.Domain.IRepositories.BlindBoxes;
using Fsel.System.Domain.IRepositories.DailyQuizs;
using Fsel.System.Infrastructure;
using Fsel.System.Infrastructure.Common;
using Fsel.System.Infrastructure.Repositories;
using Fsel.System.Infrastructure.Repositories.BlindBoxes;
using Fsel.System.Infrastructure.Repositories.DailyQuizs;
using Fsel.System.Infrastructure.ValueSettings;
using Microsoft.EntityFrameworkCore;
using Refit;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var appSetting = builder.AddAppSettings<AppSetting>();
builder.AddServices(appSetting);
builder.AddOpenIdSwaggerGens(appSetting);
builder.AddOpenIdAuthenticationJwtBearers(appSetting);
builder.AddDbContexts<SystemDbContext>();

builder.Services.AddDbContext<CrmDbContext>(
        options => options.UseSqlServer(appSetting?.ConnectionStrings?.CrmConnection));

builder.Services.AddScoped<ILiveTimeFrameRepository, LiveTimeFrameRepository>();
builder.Services.AddScoped<ICourseTimeConfigRepository, CourseTimeConfigRepository>();
builder.Services.AddScoped<IForbiddenWordRepository, ForbiddenWordRepository>();
builder.Services.AddScoped<ITeachingCostRepository, TeachingCostRepository>();
builder.Services.AddScoped<IReferralDiscountConfigRepository, ReferralDiscountConfigRepository>();
builder.Services.AddScoped<ILogActionRepository, LogActionRepository>();
builder.Services.AddScoped<IQuestBoardRepository, QuestBoardRepository>();
builder.Services.AddScoped<IQuestBoardOverallRepository, QuestBoardOverallRepository>();
builder.Services.AddScoped<IQuestBoardOverallStudentRepository, QuestBoardOverallStudentRepository>();
builder.Services.AddScoped<IFeatureAccessTimeRepository, FeatureAccessTimeRepository>();
builder.Services.AddScoped<IQuestBoardStudentRepository, QuestBoardStudentRepository>();
builder.Services.AddScoped<IGameTopicRepository, GameTopicRepository>();
builder.Services.AddScoped<IGameVocabularyRepository, GameVocabularyRepository>();
builder.Services.AddScoped<IGameVocabularyTypeRepository, GameVocabularyTypeRepository>();
builder.Services.AddScoped<IFocusTimeConfigRepository, FocusTimeRepository>();
builder.Services.AddScoped<IGameVocabularyPlatformRepository, GameVocabularyPlatformRepository>();
builder.Services.AddScoped<ITokenConfigRepository, TokenConfigRepository>();
builder.Services.AddScoped<IApprovalTimeConfigRepository, ApprovalTimeConfigRepository>();
builder.Services.AddScoped<IApprovalLogRepository, ApprovalLogRepository>();
builder.Services.AddScoped<ILocationRepository, LocationRepository>();
builder.Services.AddScoped<ISchoolRepository, SchoolRepository>();
builder.Services.AddScoped<IChatbotConfigRepository, ChatbotConfigRepository>();
builder.Services.AddScoped<IErrorReportRepository, ErrorReportRepository>();
builder.Services.AddScoped<ITokenHistoryRepository, TokenHistoryRepository>();
builder.Services.AddScoped<IChatBotRepository, ChatBotRepository>();
builder.Services.AddScoped<ICrmLocationRepository, CrmLocationRepository>();
builder.Services.AddScoped<IUserConfigRepository, UserConfigRepository>();
builder.Services.AddScoped<ITechieRepository, TechieRepository>();
builder.Services.AddScoped<ITechieActionRepository, TechieActionRepository>();
builder.Services.AddScoped<IStudentTechieRepository, StudentTechieRepository>();
builder.Services.AddScoped<ILuckyTicketRepository, LuckyTicketRepository>();
builder.Services.AddScoped<IFselRatingRepository, FselRatingRepository>();
builder.Services.AddScoped<ICourseTargetConfigRepository, CourseTargetConfigRepository>();
builder.Services.AddScoped<ICourseSuggestConfigRepository, CourseSuggestConfigRepository>();
builder.Services.AddScoped<IBannerRepository, BannerRepository>();
builder.Services.AddScoped<IBannerSettingRepository, BannerSettingRepository>();
builder.Services.AddScoped<IBannerStudentRepository, BannerStudentRepository>();
builder.Services.AddScoped<IBannerScopeRepository, BannerScopeRepository>();
builder.Services.AddScoped<IBannerImageRepository, BannerImageRepository>();
builder.Services.AddScoped<ISharePointService, SharePointService>();
builder.Services.AddScoped<IBlindBoxRepository, BlindBoxRepository>();
builder.Services.AddScoped<IBlindBoxChestRepository, BlindBoxChestRepository>();
builder.Services.AddScoped<IBlindBoxChestConfigRepository, BlindBoxChestConfigRepository>();
builder.Services.AddScoped<IBlindBoxHistoryRepository, BlindBoxHistoryRepository>();
builder.Services.AddScoped<IBlindBoxUserRepository, BlindBoxUserRepository>();
builder.Services.AddScoped<IDictionaryRepository, DictionaryRepository>();
builder.Services.AddScoped<IUnknownWordRepository, UnknownWordRepository>();

builder.Services.AddScoped<IFselRatingRepository, FselRatingRepository>();
builder.Services.AddScoped<IDisplayOrderConfigRepository, DisplayOrderConfigRepository>();

builder.Services.AddScoped<IDailyQuizAnswerRepository, DailyQuizAnswerRepository>();
builder.Services.AddScoped<IDailyQuizHistoryRepository, DailyQuizHistoryRepository>();
builder.Services.AddScoped<IDailyQuizQuestionRepository, DailyQuizQuestionRepository>();
builder.Services.AddScoped<IDailyQuizWinnerRepository, DailyQuizWinnerRepository>();

builder.Services.AddScoped<ISenderConfigRepository, SenderConfigRepository>();

builder.Services.AddScoped<SetCompleteApprovalPublisher>();
builder.Services.AddScoped<TokenConfigsConverter>();
builder.Services.AddScoped<NotificationMessagePublisher>();
builder.Services.AddScoped<ChatBotPublisher>();
builder.Services.AddScoped<TechieSendMessagePublisher>();
builder.Services.AddScoped<BannerConverter>();
builder.Services.AddScoped<BannerPublisher>();
builder.Services.AddScoped<BuyBlindBoxPublisher>();
builder.Services.AddScoped<SendNotifyBuyBlindBoxPublisher>();
builder.Services.AddScoped<DictionaryPublisher>();
builder.Services.AddScoped<CrawDictionaryDataPublisher>();

//Add GoogleSheetService
builder.Services.AddSingleton<IGoogleSheetService>(provider =>
{
    return new GoogleSheetService(ResourceSettings.I18NCredentialsFilePath);
});

builder.AddRefitClients(typeof(IUserService), appSetting?.Services?.UserApiUrl);
builder.AddRefitClients(typeof(ICourseService), appSetting?.Services?.LmsCourseApiUrl);
builder.AddRefitClients(typeof(IOrderService), appSetting?.Services?.OrderApiUrl);
builder.AddRefitClients(typeof(IDictionaryService), appSetting?.Services?.DictionaryApiUrl);
builder.AddRefitClients(typeof(ISenderService), appSetting?.Services?.SenderApiUrl);
builder.AddRefitClients(typeof(IStorageService), appSetting?.Services?.StorageApiUrl);
builder.AddRefitClients(typeof(IFFmpegServices), appSetting?.Services?.FFmpegApiUrl);
builder.Services.AddRefitClient<IOpenAIService>().ConfigureHttpClient(delegate (IServiceProvider serviceProvider, HttpClient httpClient)
{
    httpClient.BaseAddress = new Uri(appSetting?.OpenAiConfig?.Uri ?? string.Empty);
    if (!string.IsNullOrEmpty(appSetting?.OpenAiConfig?.ApiKey))
    {
        httpClient.DefaultRequestHeaders.Add("Authorization", $"{Settings.Bearer} {appSetting?.OpenAiConfig?.ApiKey}");
    }
});

builder.AddMassTransit(appSetting,
queues: new Dictionary<string, Type>
{
    { QueueSettings.SystemQueue.NameQueue.CompleteApprovalPostTimeOut, typeof(CompleteApprovalConsumer) },
    { QueueSettings.LmsQueue.NameQueue.CreateTokenHistory, typeof(CreateTokenHistoryConsumer) },
    { QueueSettings.UserQueue.NameQueue.CreateTokenHistory, typeof(CreateTokenHistoryConsumer) },
    { QueueSettings.RealtimeQueue.NameQueue.FeatureAccessTime, typeof(FeatureAccessTimeConsumer) },
    { QueueSettings.RealtimeQueue.NameQueue.ChatBot, typeof(ChatBotConsumer) },
    { QueueSettings.LmsQueue.NameQueue.DoQuestBoard, typeof(CreateTokenHistoryConsumer) },
    { QueueSettings.SystemQueue.NameQueue.QuestBoard, typeof(DoQuestBoardConsumer) },
    { QueueSettings.RealtimeQueue.NameQueue.TechieAction, typeof(TechieConsumer) },
    { QueueSettings.LmsQueue.NameQueue.Techie, typeof(TechieConsumer) },
    { QueueSettings.SystemQueue.NameQueue.CreateLuckyTicket, typeof(CreateLuckyTicketConsumer) },
    { QueueSettings.SystemQueue.NameQueue.NoticeAccessTime, typeof(NoticeAccessFeatureConsumer) },
    { QueueSettings.InteractionQueue.NameQueue.CreateTokenHistory, typeof(CreateTokenHistoryConsumer) },
    { QueueSettings.RealtimeQueue.NameQueue.Banner, typeof(BannerConsumer) },
    { QueueSettings.SystemQueue.NameQueue.BuyBlindBox, typeof(BuyBlindBoxConsumer) },
    { QueueSettings.SystemQueue.NameQueue.ChooseDailyQuizWinners, typeof(ChooseDailyQuizWinnersConsumer) },
    { QueueSettings.RealtimeQueue.NameQueue.DictionaryRealTime, typeof(DictionaryConsumer) },
    { QueueSettings.SystemQueue.NameQueue.CrawDictionaryData, typeof(CrawDictionaryDataConsumer) },
    { QueueSettings.OrderingQueue.NameQueue.AddCoinWhenCoursePurchased, typeof(AddCoinWhenCoursePurchasedConsumer) }
});

var app = builder.Build();
app.UseServices();
app.Run();
