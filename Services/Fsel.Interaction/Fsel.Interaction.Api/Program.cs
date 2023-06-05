// Copyright (c) Atlantic. All rights reserved.

using Fsel.Common.ValueSettings;
using Fsel.Core.Extensions;
using Fsel.Interaction.Application.Services.UserServices.Models;
using Fsel.Interaction.Domain.IRepositories;
using Fsel.Interaction.Infrastructure;
using Fsel.Interaction.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var appSetting = builder.AddAppSettings<BaseAppSetting>();
builder.AddServices();
builder.AddSwaggerGens(appSetting);
builder.AddAuthenticationJwtBearers(appSetting);
builder.AddDbContexts<InteractionDbContext>();

builder.Services.AddScoped<ISurveyQuestionRepository, SurveyQuestionRepository>();
builder.Services.AddScoped<ICustomerSurveyRepository, CustomerSurveyRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<IInteractionActionRepository, InteractionActionRepository>();

builder.AddRefitClients(typeof(IUserService), appSetting?.Services?.UserApiUrl);
var app = builder.Build();

app.UseServices();
app.Run();
