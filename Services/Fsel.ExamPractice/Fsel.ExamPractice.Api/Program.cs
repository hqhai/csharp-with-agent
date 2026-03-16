// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Extensions;
using Fsel.ExamPractice.Application.Services.SystemServices;
using Fsel.ExamPractice.Domain.IRepositories;
using Fsel.ExamPractice.Infrastructure;
using Fsel.ExamPractice.Infrastructure.Common;
using Fsel.ExamPractice.Infrastructure.Common.ExamPracticeHelpers;
using Fsel.ExamPractice.Infrastructure.Repositories;
using Fsel.ExamPractice.Infrastructure.ValueSettings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var appSetting = builder.AddAppSettings<AppSetting>();
builder.AddServices(appSetting);
builder.AddOpenIdSwaggerGens(appSetting);
builder.AddOpenIdAuthenticationJwtBearers(appSetting);
builder.AddDbContexts<ExamPracticesDBContext, ExamPracticesReadDBContext>();

builder.Services.AddScoped<IExamPracticeAnswerRepository, ExamPracticeAnswerRepository>();
builder.Services.AddScoped<IExamPracticeRepository, ExamPracticeRepository>();
builder.Services.AddScoped<IExamPracticeResultRepository, ExamPracticeResultRepository>();
builder.Services.AddScoped<IExamPracticeRetryRepository, ExamPracticeRetryRepository>();
builder.Services.AddScoped<IExamPracticeSectionRepository, ExamPracticeSectionRepository>();
builder.Services.AddScoped<IExamPracticeSectionResultRepository, ExamPracticeSectionResultRepository>();
builder.Services.AddScoped<IExamPracticeAISettingRepository, ExamPracticeAISettingRepository>();
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
builder.Services.AddScoped<IExamPracticeAICriteriaSettingRepository, ExamPracticeAICriteriaSettingRepository>();

builder.Services.AddScoped<ExamPracticeHelper>();
builder.Services.AddScoped<ExamPracticeConverter>();
builder.Services.AddScoped<ExamPracticeCommon>();

builder.AddRefitClients(typeof(ISystemService), appSetting?.Services?.SystemApiUrl);
var app = builder.Build();
app.UseServices();
app.Run();
