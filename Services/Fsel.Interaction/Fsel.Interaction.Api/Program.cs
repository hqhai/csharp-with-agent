// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.Constants;
using Fsel.Common.ValueSettings;
using Fsel.Core.Extensions;
using Fsel.Interaction.Application.Queues.Consumers;
using Fsel.Interaction.Application.Queues.Publishers;
using Fsel.Interaction.Application.Services.AIService;
using Fsel.Interaction.Application.Services.CourseServices;
using Fsel.Interaction.Application.Services.HarmfulContentService;
using Fsel.Interaction.Application.Services.NotificationService;
using Fsel.Interaction.Application.Services.OrderService;
using Fsel.Interaction.Application.Services.SenderServices;
using Fsel.Interaction.Application.Services.SystemService;
using Fsel.Interaction.Application.Services.TrainingServices;
using Fsel.Interaction.Application.Services.UserServices;
using Fsel.Interaction.Domain.IRepositories;
using Fsel.Interaction.Infrastructure;
using Fsel.Interaction.Infrastructure.Repositories;
using Fsel.Interaction.Infrastructure.ValueSettings;
using Fsel.Shared.Constants;
using Refit;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var appSetting = builder.AddAppSettings<AppSetting>();
builder.AddServices(appSetting);
builder.AddOpenIdSwaggerGens(appSetting);
builder.AddOpenIdAuthenticationJwtBearers(appSetting);
builder.AddDbContexts<InteractionDbContext>();

builder.Services.AddScoped<ISurveyQuestionRepository, SurveyQuestionRepository>();
builder.Services.AddScoped<ICustomerSurveyRepository, CustomerSurveyRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<IInteractionActionRepository, InteractionActionRepository>();
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<IPostTagRepository, PostTagRepository>();
builder.Services.AddScoped<ITopicTagRepository, TopicTagRepository>();
builder.Services.AddScoped<IStudentReviewRepository, StudentReviewRepository>();
builder.Services.AddScoped<ISupportCategoryRepository, SupportCategoryRepository>();
builder.Services.AddScoped<ISupportQuestionRepository, SupportQuestionRepository>();
builder.Services.AddScoped<ISupportTicketRepository, SupportTicketRepository>();
builder.Services.AddScoped<ISupportTicketRepository, SupportTicketRepository>();
builder.Services.AddScoped<IFlagRepository, FlagRepository>();
builder.Services.AddScoped<ICustomerSurveyGroupRepository, CustomerSurveyGroupRepository>();
builder.Services.AddScoped<ISurveyConfigRepository, SurveyConfigRepository>();
builder.Services.AddScoped<IUserSurveyAssignmentRepository, UserSurveyAssignmentRepository>();

builder.Services.AddScoped<DiscussionBoardCommentPublisher>();
builder.Services.AddScoped<DiscussionBoardLikePublisher>();
builder.Services.AddScoped<InterationActionPublisher>();
builder.Services.AddScoped<NotificationMessagePublisher>();
builder.Services.AddScoped<DeleteClassForumByFlagPublisher>();
builder.Services.AddScoped<CompleteApprovalPostPublisher>();
builder.Services.AddScoped<QuestBoardPublisher>();
builder.Services.AddScoped<CreateTokenHistoryPublisher>();
builder.Services.AddScoped<SendNotifyUserHasSurveyPublisher>();

builder.AddRefitClients(typeof(IUserService), appSetting?.Services?.UserApiUrl);
builder.AddRefitClients(typeof(ITrainingService), appSetting?.Services?.TrainingApiUrl);
builder.AddRefitClients(typeof(ICourseService), appSetting?.Services?.LmsCourseApiUrl);
builder.AddRefitClients(typeof(ISenderService), appSetting?.Services?.SenderApiUrl);
builder.AddRefitClients(typeof(INotificationService), appSetting?.Services?.NotificationApiUrl);
builder.AddRefitClients(typeof(ISystemService), appSetting?.Services?.SystemApiUrl);
builder.AddRefitClients(typeof(IOrderService), appSetting?.Services?.OrderApiUrl);

builder.Services.AddRefitClient<IOpenAIService>().ConfigureHttpClient(delegate (IServiceProvider serviceProvider, HttpClient httpClient)
{
    httpClient.BaseAddress = new Uri(appSetting?.OpenAiConfig?.Uri ?? string.Empty);
    if (!string.IsNullOrEmpty(appSetting?.OpenAiConfig?.ApiKey))
    {
        httpClient.DefaultRequestHeaders.Add("Authorization", $"{Settings.Bearer} {appSetting?.OpenAiConfig?.ApiKey}");
    }
});

builder.Services.AddRefitClient<IHarmfulContentWordsService>().ConfigureHttpClient(delegate (IServiceProvider serviceProvider, HttpClient httpClient)
{
    httpClient.BaseAddress = new Uri(appSetting?.HarmfulContentConfigs?.HarmfulContentWordsConfig?.HarmfulContentApiUrl ?? string.Empty);
    if (!string.IsNullOrEmpty(appSetting?.HarmfulContentConfigs?.HarmfulContentWordsConfig?.SubscriptionKey))
    {
        httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", $"{appSetting?.HarmfulContentConfigs?.HarmfulContentWordsConfig.SubscriptionKey}");
    }
});

builder.Services.AddRefitClient<IHarmfulContentImageService>().ConfigureHttpClient(delegate (IServiceProvider serviceProvider, HttpClient httpClient)
{
    httpClient.BaseAddress = new Uri(appSetting?.HarmfulContentConfigs?.HarmfulContentImageConfig?.HarmfulContentApiUrl ?? string.Empty);
    if (!string.IsNullOrEmpty(appSetting?.HarmfulContentConfigs?.HarmfulContentImageConfig?.SubscriptionKey))
    {
        httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", $"{appSetting?.HarmfulContentConfigs?.HarmfulContentImageConfig.SubscriptionKey}");
    }
});

builder.AddMassTransit(appSetting,
queues: new Dictionary<string, Type>
{
    { QueueSettings.InteractionQueue.NameQueue.SaveUserSurveyAssignment, typeof(SaveUserSurveyAssignmentConsumer) },
    { QueueSettings.InteractionQueue.NameQueue.SendNotifyUserHasSurvey, typeof(SendNotifyUserHasSurveyConsumer) },
});

var app = builder.Build();
app.UseServices();
app.Run();
