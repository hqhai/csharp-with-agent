// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Extensions;
using Fsel.ExamPractice.Application.Services.SystemServices;
using Fsel.ExamPractice.Domain.IRepositories;
using Fsel.ExamPractice.Infrastructure;
using Fsel.ExamPractice.Infrastructure.Common;
using Fsel.ExamPractice.Infrastructure.Common.ExamPracticeHelpers;
using Fsel.ExamPractice.Infrastructure.Common.Processors;
using Fsel.ExamPractice.Infrastructure.Common.Validators;
using Fsel.ExamPractice.Infrastructure.Repositories;
using Fsel.ExamPractice.Infrastructure.ValueSettings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var appSetting = builder.AddAppSettings<AppSetting>();
builder.AddServices(appSetting);
builder.AddOpenIdSwaggerGens(appSetting);
builder.AddOpenIdAuthenticationJwtBearers(appSetting);
builder.AddDbContexts<ExamPracticesDBContext, ExamPracticesReadDBContext>();

// Register repositories
builder.Services.AddScoped<IExamPracticeAnswerRepository, ExamPracticeAnswerRepository>();
builder.Services.AddScoped<IExamPracticeRepository, ExamPracticeRepository>();
builder.Services.AddScoped<IExamPracticeResultRepository, ExamPracticeResultRepository>();
builder.Services.AddScoped<IExamPracticeRetryRepository, ExamPracticeRetryRepository>();
builder.Services.AddScoped<IExamPracticeSectionRepository, ExamPracticeSectionRepository>();
builder.Services.AddScoped<IExamPracticeSectionResultRepository, ExamPracticeSectionResultRepository>();
builder.Services.AddScoped<IExamPracticeAISettingRepository, ExamPracticeAISettingRepository>();
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
builder.Services.AddScoped<IExamPracticeAICriteriaSettingRepository, ExamPracticeAICriteriaSettingRepository>();

// Register ExamPracticeHelper
builder.Services.AddScoped<ExamPracticeHelper>();

// Register ExamPracticeCommon - transient vì mỗi request cần instance mới
builder.Services.AddTransient<ExamPracticeCommon>();

// Register Validators by type
builder.Services.AddScoped<IUpdateExamPracticeValidator, ExamPracticeTypeValidator>();
builder.Services.AddScoped<IUpdateExamPracticeValidator, IeltsTypeValidator>();
builder.Services.AddScoped<IUpdateExamPracticeValidator, VstepTypeValidator>();

// Register Processors by type
builder.Services.AddScoped<IUpdateExamPracticeProcessor, ExamPracticeTypeProcessor>();
builder.Services.AddScoped<IUpdateExamPracticeProcessor, IeltsTypeProcessor>();
builder.Services.AddScoped<IUpdateExamPracticeProcessor, VstepTypeProcessor>();

// Register Refit clients
builder.AddRefitClients(typeof(ISystemService), appSetting?.Services?.SystemApiUrl);

var app = builder.Build();
app.UseServices();
app.Run();
