using Fsel.Common.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Ocelot.Middleware;

namespace Fsel.Core.Extensions
{
    public static class StartupAppExtensions
    {
        public static void UseServices(this WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseRouting();
            app.UseHttpsRedirection();

            app.UseDefaultServices();
        }

        public static void UseGatewayServices(this WebApplication app)
        {
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerForOcelotUI();
            }

            app.UseHttpsRedirection();
            app.UseOcelot().Wait();

            app.UseDefaultServices();
        }

        private static void UseDefaultServices(this WebApplication app)
        {
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.UseCors();
        }

        private static void UseCors(this WebApplication app)
        {
            app.UseCors();
            app.UseCors(Settings.CorsPolicy);
        }
    }
}
