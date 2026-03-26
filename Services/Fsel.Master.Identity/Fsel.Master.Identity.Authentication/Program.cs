// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Extensions;
using Fsel.Master.Identity.Domain.Entities;
using Fsel.Master.Identity.Domain.IRepositories;
using Fsel.Master.Identity.Infrastructure;
using Fsel.Master.Identity.Infrastructure.Repositories;
using Fsel.Master.Identity.Infrastructure.ValueSettings;

var builder = WebApplication.CreateBuilder(args);

var appSetting = builder.AddAppSettings<AppSetting>();
builder.AddServices(appSetting);
builder.AddSwaggerGens(appSetting);
builder.AddConfigureIdentityOptions();
builder.AddIdentity<MasterUser, MasterRole, UserMasterDBContext>();
builder.AddAuthenticationJwtBearers(appSetting);
builder.AddDbContexts<UserMasterDBContext, UserMasterReadDbContext, MasterUser, MasterRole, MasterUserClaim, MasterUserRole, MasterUserLogin, MasterUserToken, MasterRoleClaim>();

builder.Services.AddScoped<IMasterUserTokenRepository, MasterUserTokenRepository>();

builder.AddMassTransit(appSetting,
queues: new Dictionary<string, Type>
{
});

var app = builder.Build();
app.UseServices();
app.Run();
