using Fsel.Core.Extensions;
using Fsel.Identity.Application.Services;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.IRepositories;
using Fsel.Identity.Infrastructure;
using Fsel.Identity.Infrastructure.Repositories;
using Fsel.Identity.Infrastructure.ValueSettings;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);
var appSetting = builder.AddAppSettings<AppSetting>();
builder.AddServices();
builder.AddSwaggerGens(appSetting);
builder.AddAuthentication();
builder.AddDbContexts<UserDbContext>();

builder.Services.AddIdentity<User, Role>()
        .AddEntityFrameworkStores<UserDbContext>()
        .AddDefaultTokenProviders();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IHumanRepository, HumanRepository>();
builder.Services.AddScoped<ITeacherRepository, TeacherRepository>();
builder.Services.AddScoped<IParentRepository, ParentRepository>();
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<IParentStudentRepository, ParentStudentRepository>();
builder.Services.AddCors(policy =>
{
    policy.AddPolicy("OpenCorsPolicy", opt => opt.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});
builder.AddRefitClients(typeof(ISenderService), appSetting?.Services?.SenderApiUrl);

//App config
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
//app configurations

app.UseHttpsRedirection();
app.UseCors("OpenCorsPolicy");
app.UseAuthorization();

app.MapControllers();

app.Run();
