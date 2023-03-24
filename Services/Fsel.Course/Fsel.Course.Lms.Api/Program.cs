using Fsel.Common.ValueSettings;
using Fsel.Core.Extensions;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Infrastructure;
using Fsel.Course.Infrastructure.Repositories;
using Fsel.Course.Lms.Application.Services.StudentServices;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var appSetting = builder.AddAppSettings<BaseAppSetting>();
builder.AddServices();
builder.AddSwaggerGens(appSetting);
builder.AddAuthenticationJwtBearers(appSetting);
builder.AddDbContexts<CourseDbContext>();

builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<ICourseTeacherRepository, CourseTeacherRepository>();
builder.Services.AddScoped<ICourseUnitMockTestRepository, CourseUnitMockTestRepository>();
builder.Services.AddScoped<ICourseClassRepository, CourseClassRepository>();
builder.AddRefitClients(typeof(IStudentService), appSetting?.Services?.UserApiUrl);

var app = builder.Build();

app.UseServices();
app.Run();
