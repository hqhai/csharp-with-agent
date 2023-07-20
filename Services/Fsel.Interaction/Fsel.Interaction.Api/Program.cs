// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Extensions;
using Fsel.Interaction.Application.Services.CourseServices;
using Fsel.Interaction.Application.Services.TrainingService;
using Fsel.Interaction.Application.Services.UserServices;
using Fsel.Interaction.Domain.IRepositories;
using Fsel.Interaction.Infrastructure;
using Fsel.Interaction.Infrastructure.Repositories;
using Fsel.Interaction.Infrastructure.ValueSettings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var appSetting = builder.AddAppSettings<AppSetting>();
builder.AddServices();
builder.AddSwaggerGens(appSetting);
builder.AddAuthenticationJwtBearers(appSetting);
builder.AddDbContexts<InteractionDbContext>();

builder.Services.AddScoped<ISurveyQuestionRepository, SurveyQuestionRepository>();
builder.Services.AddScoped<ICustomerSurveyRepository, CustomerSurveyRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<IInteractionActionRepository, InteractionActionRepository>();
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<IPostTagRepository, PostTagRepository>();
builder.Services.AddScoped<ITopicTagRepository, TopicTagRepository>();
builder.Services.AddScoped<IStudentReviewRepository, StudentReviewRepository>();

builder.AddRefitClients(typeof(IUserService), appSetting?.Services?.UserApiUrl);
builder.AddRefitClients(typeof(ITrainingService), appSetting?.Services?.TrainingApiUrl);
builder.AddRefitClients(typeof(ICourseService), appSetting?.Services?.LmsCourseApiUrl);
var app = builder.Build();

app.UseServices();
app.Run();
