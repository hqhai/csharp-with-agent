// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Authentication.Quickstart
{
    using Amazon.SimpleEmail.Model;
    using Fsel.Common.Helpers;
    using Microsoft.AspNetCore.Html;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.Extensions.Localization;
    using Nest;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text.RegularExpressions;

    public static class CustomHtmlHelperExtensions
    {
        private static string? GetErrorMessageFromDataAnnotations(this ModelMetadata metadata, string propertyName)
        {
            var property = metadata.ModelType.GetProperty(propertyName);
            if (property == null)
            {
                return string.Empty;
            }

            var attributes = property.GetCustomAttributes<ValidationAttribute>(true);
            foreach (var attribute in attributes)
            {
                if (!string.IsNullOrEmpty(attribute.ErrorMessage))
                {
                    return attribute.ErrorMessage;
                }
            }

            return string.Empty;
        }

        public static IHtmlContent CustomValidationMessageFor<TModel, TResult>(
        this IHtmlHelper<TModel> htmlHelper,
        Expression<Func<TModel, TResult>> expression,
        IStringLocalizer stringLocalizer,
        string message,
        object htmlAttributes)
        {
            ArgumentNullException.ThrowIfNull(htmlHelper);
            ArgumentNullException.ThrowIfNull(expression);
            ArgumentNullException.ThrowIfNull(stringLocalizer);

            // Lấy thông báo lỗi từ ValidationMessageFor
            var validationMessage = htmlHelper.ValidationMessageFor(expression, message, htmlAttributes);

            // Lấy tên thuộc tính từ biểu thức lambda
            var memberExpression = expression.Body as MemberExpression;
            ArgumentNullException.ThrowIfNull(memberExpression);

            var propertyName = memberExpression.Member.Name;
            var errorMessage = htmlHelper.ViewData.ModelMetadata.GetErrorMessageFromDataAnnotations(propertyName);

            var tagBuilder = new TagBuilder("div");
            tagBuilder.InnerHtml.AppendHtml(validationMessage);

            if (!string.IsNullOrEmpty(errorMessage))
            {
                tagBuilder.InnerHtml.AppendHtml($"<div class=\"validation-message-text hidden\" data-field=\"{propertyName}\">{stringLocalizer[errorMessage]}</div>");
            }

            return tagBuilder;
        }

        public static string? ToLowerFirstChar(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            Span<char> span = input.ToCharArray();
            span[0] = char.ToLower(span[0], culture: CultureInfo.InvariantCulture);
            return new string(span);
        }
    }
}
