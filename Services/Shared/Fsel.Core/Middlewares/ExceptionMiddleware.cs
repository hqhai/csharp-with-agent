// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Core.Middlewares
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Helpers;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Diagnostics;
    using Microsoft.AspNetCore.Http;

    public static class ExceptionMiddleware
    {
        public static void ExceptionMiddlewareHandler(this IApplicationBuilder app)
        {
            app.UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    context.Response.ContentType = "application/json";
                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (contextFeature != null)
                    {
                        VoidMethodResult errorResult = new VoidMethodResult();
                        errorResult.AddErrorServer();
                        await context.Response.WriteAsync(errorResult.Serialize());
                    }
                });
            });
        }
    }
}
