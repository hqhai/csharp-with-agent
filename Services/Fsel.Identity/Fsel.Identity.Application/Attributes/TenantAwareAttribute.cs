// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Attributes
{
    using System.Linq;
    using System.Text.Encodings.Web;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;
    using Fsel.Common.Constants;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Identity.Domain.Models.CommandModels.OpenId;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.Filters;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class TenantAwareAttribute : TypeFilterAttribute
    {
        public TenantAwareAttribute() : base(typeof(TenantAwareFilter)) { }
    }

    public class TenantAwareFilter : IAsyncResourceFilter, IOrderedFilter
    {
        private readonly ITenantProvider _tenantProvider;
        public int Order => int.MinValue;

        public TenantAwareFilter(ITenantProvider tenantProvider) => _tenantProvider = tenantProvider;

        public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(next);

            if (context?.HttpContext?.Request == null)
            {
                await next.Invoke();
                return;
            }

            var (username, userId) = await ExtractTenantInfoAsync(context);
            if (!string.IsNullOrWhiteSpace(username) || userId.HasValue)
            {
                var tenant = await _tenantProvider.GetTenantAsync(username, userId);
                if (tenant == null)
                {
                    await next.Invoke();
                    return;
                }

                var tenantByDomain = await _tenantProvider.GetTenantAsync(isCheckDefault: false);
                if (tenant.IsMultiLogin || (tenantByDomain != null && tenant.Id == tenantByDomain.Id))
                {
                    context.HttpContext.SetHeader(JwtClaimNames.TenantId, tenant.Id.ToString());
                }
            }

            await next.Invoke();
        }

        private static async Task<(string? username, Guid? userId)> ExtractTenantInfoAsync(ResourceExecutingContext context)
        {
            var request = context.HttpContext.Request;
            string? identity = null;
            string? username = null;
            Guid? userId = null;

            if (request.HasFormContentType)
            {
                var form = await request.ReadFormAsync(context.HttpContext.RequestAborted);
                identity = form[nameof(IRequestBodyTenantAware.Identity)].FirstOrDefault();
                username = form[nameof(IRequestBodyTenantAware.UserName)].FirstOrDefault();
                userId = form[nameof(IRequestBodyTenantAware.UserId)].FirstOrDefault().Parse<Guid?>();
            }
            else
            {
                var tenantParam = context.ActionDescriptor.Parameters
                    .OfType<ParameterDescriptor>()
                    .FirstOrDefault(p => typeof(IRequestBodyTenantAware).IsAssignableFrom(p.ParameterType));

                if (tenantParam != null && HttpMethods.IsPost(request.Method) || HttpMethods.IsPut(request.Method) || HttpMethods.IsPatch(request.Method))
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
                    var model = await JsonSerializer.DeserializeAsync(request.Body, tenantParam!.ParameterType, jsonSerializerOptions, context.HttpContext.RequestAborted);
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
                username = context.HttpContext.GetValue<string>(nameof(IRequestBodyTenantAware.UserName));
            }
            if (!userId.HasValue)
            {
                userId = context.HttpContext.GetValue<Guid?>(nameof(IRequestBodyTenantAware.UserId));
            }

            return (username, userId);
        }
    }
}


