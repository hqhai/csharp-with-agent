// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Middlewares
{
    using System;
    using System.Linq;
    using System.Text.Encodings.Web;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using Fsel.Common.Constants;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Core.Extensions;
    using Fsel.Identity.Application.Attributes;
    using Fsel.Identity.Domain.Models.CommandModels.OpenId;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.Extensions.DependencyInjection;

    public sealed class TenantAwareMiddleware
    {
        private readonly RequestDelegate _next;

        public TenantAwareMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IServiceProvider serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(serviceProvider);

            var endpoint = context.GetEndpoint();
            var actionDescriptor = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();
            var hasTenantAwareAttribute = actionDescriptor != null &&
                                          (actionDescriptor.MethodInfo.GetCustomAttributes(typeof(TenantAwareAttribute), true).Any() ||
                                           actionDescriptor.ControllerTypeInfo.GetCustomAttributes(typeof(TenantAwareAttribute), true).Any());

            if (!hasTenantAwareAttribute)
            {
                await _next(context);
                return;
            }

            var (username, userId) = await ExtractTenantInfoAsync(context, actionDescriptor);

            if (string.IsNullOrWhiteSpace(username) && userId == null)
            {
                await _next(context);
                return;
            }

            var tenantProvider = serviceProvider.GetRequiredService<ITenantProvider>();
            var tenant = await tenantProvider.GetTenantAsync(username, userId);
            if (tenant != null)
            {
                var tenantByDomain = await tenantProvider.GetTenantByDomainUrlAsync();
                if (tenantByDomain != null && (tenantByDomain.IsMultiLogin || tenant.Id == tenantByDomain.Tenant?.Id))
                {
                    var authContext = context.RequestServices.GetService<AuthContext>();
                    if (authContext != null)
                    {
                        authContext.TenantId = tenant.Id;
                    }
                    context.SetHeader(JwtClaimNames.TenantId, tenant.Id.ToString());
                }
            }

            await _next(context);
        }

        private static async Task<(string? username, Guid? userId)> ExtractTenantInfoAsync(HttpContext httpContext, ControllerActionDescriptor? actionDescriptor)
        {
            var request = httpContext.Request;

            string? identity = null;
            string? username = null;
            Guid? userId = null;

            if (request.HasFormContentType)
            {
                var form = await request.ReadFormAsync(httpContext.RequestAborted);
                identity = form[nameof(IRequestBodyTenantAware.Identity)].FirstOrDefault();
                username = form[nameof(IRequestBodyTenantAware.UserName)].FirstOrDefault();
                userId = form[nameof(IRequestBodyTenantAware.UserId)].FirstOrDefault().Parse<Guid?>();
            }
            else
            {
                var tenantParam = actionDescriptor?.Parameters
                    .OfType<ControllerParameterDescriptor>()
                    .FirstOrDefault(p => typeof(IRequestBodyTenantAware).IsAssignableFrom(p.ParameterInfo.ParameterType));

                if (tenantParam != null && (HttpMethods.IsPost(request.Method) || HttpMethods.IsPut(request.Method) || HttpMethods.IsPatch(request.Method)))
                {
                    request.EnableBuffering();
                    var jsonSerializerOptions = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        ReferenceHandler = ReferenceHandler.IgnoreCycles,
                        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    };

                    var model = await JsonSerializer.DeserializeAsync(
                        request.Body,
                        tenantParam.ParameterInfo.ParameterType,
                        jsonSerializerOptions,
                        httpContext.RequestAborted);

                    request.Body.Position = 0;

                    if (model is IRequestBodyTenantAware tenantAware)
                    {
                        identity = tenantAware.Identity;
                        username = tenantAware.UserName;
                        userId = tenantAware.UserId;
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(identity))
            {
                if (Guid.TryParse(identity, out var identityUserId))
                {
                    userId = identityUserId;
                }
                else
                {
                    username = identity;
                }
            }

            if (string.IsNullOrWhiteSpace(username))
            {
                username = httpContext.GetValue<string>(nameof(IRequestBodyTenantAware.UserName));
            }

            if (!userId.HasValue)
            {
                userId = httpContext.GetValue<Guid?>(nameof(IRequestBodyTenantAware.UserId));
            }

            return (username, userId);
        }
    }

    public static class TenantAwareMiddlewareExtensions
    {
        public static IApplicationBuilder UseTenantAwareMiddlewareHandler(this IApplicationBuilder app)
        {
            ArgumentNullException.ThrowIfNull(app);
            return app.UseMiddleware<TenantAwareMiddleware>();
        }
    }
}


