// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Base;
using Fsel.Core.Base.Interfaces;
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
builder.AddSwaggerGens(appSetting);
builder.AddAuthenticationJwtBearers(appSetting);
builder.AddDbContexts<ExamPracticesDBContext>();
builder.Services.AddScoped<IExamPracticeAnswerRepository, ExamPracticeAnswerRepository>();
builder.Services.AddScoped<IExamPracticeRepository, ExamPracticeRepository>();
builder.Services.AddScoped<IExamPracticeResultRepository, ExamPracticeResultRepository>();
builder.Services.AddScoped<IExamPracticeRetryRepository, ExamPracticeRetryRepository>();
builder.Services.AddScoped<IExamPracticeSectionRepository, ExamPracticeSectionRepository>();
builder.Services.AddScoped<IExamPracticeSectionResultRepository, ExamPracticeSectionResultRepository>();
builder.Services.AddScoped<IExamPracticeAISettingRepository, ExamPracticeAISettingRepository>();
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
builder.Services.AddScoped<IExamPracticeAICriteriaSettingRepository, ExamPracticeAICriteriaSettingRepository>();

builder.Services.AddScoped(provider =>
{
    return provider.GetRequiredService<ExamPracticesDBContext>() as BaseDbContext;
});
builder.Services.AddScoped(typeof(IRepository<>), typeof(BaseRepository<>));
builder.Services.AddScoped(typeof(IVersionEntityUpdater<>), typeof(EntityVersionUpdater<>));

builder.Services.AddScoped<ExamPracticeHelper>();
builder.Services.AddScoped<ExamPracticeConverter>();

builder.AddRefitClients(typeof(ISystemService), appSetting?.Services?.SystemApiUrl);
var app = builder.Build();
app.UseServices();
app.Run();
