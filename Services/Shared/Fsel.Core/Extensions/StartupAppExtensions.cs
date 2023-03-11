using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

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

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
        }
    }
}