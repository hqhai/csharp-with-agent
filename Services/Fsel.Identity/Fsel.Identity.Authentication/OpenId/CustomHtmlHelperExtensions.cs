// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Authentication.OpenId
{
    using Microsoft.AspNetCore.Html;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.Extensions.Localization;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.Linq.Expressions;
    using System.Reflection;

    public static class CustomHtmlHelperExtensions
    {
        private static Dictionary<string, string> GetErrorMessageFromDataAnnotations(this ModelMetadata metadata, string propertyName)
        {
            var messages = new Dictionary<string, string>();
            var property = metadata.ModelType.GetProperty(propertyName);
            if (property == null)
            {
                return messages;
            }

            var attributes = property.GetCustomAttributes<ValidationAttribute>(true);
            if (attributes != null)
            {
                foreach (var attribute in attributes)
                {
                    switch (attribute)
                    {
                        case RequiredAttribute requiredAttribute:
                            messages.Add("data-val-required", requiredAttribute.ErrorMessage ?? string.Empty);
                            break;

                        case RegularExpressionAttribute regexAttribute:
                            messages.Add("data-val-regex", regexAttribute.ErrorMessage ?? string.Empty);
                            //messages.Add("data-val-regex-pattern", regexAttribute.Pattern ?? string.Empty);
                            break;

                        case MaxLengthAttribute maxLengthAttribute:
                            //messages.Add("data-val-length-max", maxLengthAttribute.Length.ToString(CultureInfo.InvariantCulture));
                            messages.Add("data-val-length", maxLengthAttribute.ErrorMessage ?? string.Empty);
                            break;

                        case MinLengthAttribute minLengthAttribute:
                            //messages.Add("data-val-length-min", minLengthAttribute.Length.ToString(CultureInfo.InvariantCulture));
                            messages.Add("data-val-length", minLengthAttribute.ErrorMessage ?? string.Empty);
                            break;

                        case RangeAttribute rangeAttribute:
                            messages.Add("data-val-range", rangeAttribute.ErrorMessage ?? string.Empty);
                            //messages.Add("data-val-range-min", rangeAttribute.Minimum.ToString() ?? string.Empty);
                            //messages.Add("data-val-range-max", rangeAttribute.Maximum.ToString() ?? string.Empty);
                            break;

                        case EmailAddressAttribute emailAttribute:
                            messages.Add("data-val-email", emailAttribute.ErrorMessage ?? string.Empty);
                            break;

                        case PhoneAttribute phoneAttribute:
                            messages.Add("data-val-phone", phoneAttribute.ErrorMessage ?? string.Empty);
                            break;

                        case CompareAttribute compareAttribute:
                            messages.Add("data-val-equalto", compareAttribute.ErrorMessage ?? string.Empty);
                            //messages.Add("data-val-equalto-other", compareAttribute.OtherProperty);
                            break;

                        case StringLengthAttribute stringLengthAttribute:
                            if (stringLengthAttribute.MaximumLength > 0)
                            {
                                //messages.Add("data-val-length-max", stringLengthAttribute.MaximumLength.ToString(CultureInfo.InvariantCulture));
                            }
                            if (stringLengthAttribute.MinimumLength > 0)
                            {
                                //messages.Add("data-val-length-min", stringLengthAttribute.MinimumLength.ToString(CultureInfo.InvariantCulture));
                            }
                            messages.Add("data-val-length", stringLengthAttribute.ErrorMessage ?? string.Empty);
                            break;

                        case CreditCardAttribute creditCardAttribute:
                            messages.Add("data-val-creditcard", creditCardAttribute.ErrorMessage ?? string.Empty);
                            break;

                        case UrlAttribute urlAttribute:
                            messages.Add("data-val-url", urlAttribute.ErrorMessage ?? string.Empty);
                            break;

                        default:
                            break;
                    }
                }

            }

            return messages;
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
            var errorMessages = htmlHelper.ViewData.ModelMetadata.GetErrorMessageFromDataAnnotations(propertyName);

            var tagBuilder = new TagBuilder("div");
            tagBuilder.AddCssClass("line-height-20");
            tagBuilder.InnerHtml.AppendHtml(validationMessage);

            foreach (var errorMessage in errorMessages)
            {
                if (!string.IsNullOrEmpty(errorMessage.Key) && !string.IsNullOrEmpty(errorMessage.Value))
                {
                    tagBuilder.InnerHtml.AppendHtml($"<div class=\"validation-message-text hidden\" data-field=\"{propertyName}\" data-validate=\"{errorMessage.Key}\">{stringLocalizer[errorMessage.Value]}</div>");
                }
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
