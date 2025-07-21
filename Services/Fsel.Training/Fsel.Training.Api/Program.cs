// Copyright (c) Atlantic. All rights reserved.

using Fsel.Core.Extensions;
using Fsel.Shared.Constants;
using Fsel.Training.Application.Queues.Consumers;
using Fsel.Training.Application.Queues.Publishers;
using Fsel.Training.Application.Services.CourseServices;
using Fsel.Training.Application.Services.OrderServices;
using Fsel.Training.Application.Services.SystemServices;
using Fsel.Training.Application.Services.UserServices;
using Fsel.Training.Domain.IRepositories;
using Fsel.Training.Infrastructure;
using Fsel.Training.Infrastructure.Repositories;
using Fsel.Training.Infrastructure.ValueSettings;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Add services to the container.
var appSetting = builder.AddAppSettings<AppSetting>();
builder.AddServices(appSetting);
builder.AddSwaggerGens(appSetting);
builder.AddAuthenticationJwtBearers(appSetting);
builder.AddDbContexts<TrainingDbContext, TrainingReadDbContext>();
builder.Services.AddScoped<IClassRepository, ClassRepository>();
builder.Services.AddScoped<IClassStudentRepository, ClassStudentRepository>();
builder.Services.AddScoped<IClassLiveCalendarRepository, ClassLiveCalendarRepository>();
builder.Services.AddScoped<ITeacherFreeDateRepository, TeacherFreeDateRepository>();
builder.Services.AddScoped<ITeacherFreeTimeRepository, TeacherFreeTimeRepository>();
builder.Services.AddScoped<IClassLiveWorkFlowRepository, ClassLiveWorkFlowRepository>();
builder.Services.AddScoped<IClassLiveWorkFlowPlanRepository, ClassLiveWorkFlowPlanRepository>();
builder.Services.AddScoped<ITeacherFreeTimeLiveRepository, TeacherFreeTimeLiveRepository>();

builder.Services.AddScoped<NotificationMessagePublisher>();
builder.Services.AddScoped<SaveUserCourseSettingPublisher>();

builder.AddRefitClients(typeof(IUserService), appSetting?.Services?.UserApiUrl);
builder.AddRefitClients(typeof(ICourseService), appSetting?.Services?.CourseApiUrl);
builder.AddRefitClients(typeof(IOrderService), appSetting?.Services?.OrderApiUrl);
builder.AddRefitClients(typeof(ISystemService), appSetting?.Services?.SystemApiUrl);

builder.AddMassTransit(appSetting,
queues: new Dictionary<string, Type>
{
    { QueueSettings.TrainingQueue.NameQueue.UpdateClassLiveAssignment, typeof(UpdateClassLiveAssignmentConsumer) },
});
//builder.Services.AddMediator(cfg =>
//{
//    cfg.AddConsumer<UpdateClassLiveAssignmentConsumer>();
//});
var app = builder.Build();
app.UseServices();
app.Run();
