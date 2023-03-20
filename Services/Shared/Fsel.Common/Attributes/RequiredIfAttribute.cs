// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Common.Attributes
{
    using System.ComponentModel.DataAnnotations;

    public sealed class RequiredIfAttribute : ValidationAttribute
    {
        public string? PropertyName { get; }
        public object? Value { get; }

        public RequiredIfAttribute(string propertyName, object value = null, string errorMessage = "")
        {
            PropertyName = propertyName;
            Value = value;
            ErrorMessage = errorMessage;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (PropertyName == null || string.IsNullOrEmpty(PropertyName))
            {
                throw new Exception("RequiredIf: you have to indicate the name of the property to use in the validation");
            }

            if (validationContext != null)
            {
                var propertyValue = GetPropertyValue(validationContext);

                if (HasPropertyValue(propertyValue) && (value == null || string.IsNullOrEmpty(value.ToString())))
                {
                    return new ValidationResult(ErrorMessage);
                }
                return base.IsValid(value, validationContext);
            }

            return default;
        }

        private object? GetPropertyValue(ValidationContext validationContext)
        {
            var instance = validationContext.ObjectInstance;
            var type = instance.GetType();
            return type.GetProperty(PropertyName)?.GetValue(instance);
        }

        private bool HasPropertyValue(object? propertyValue)
        {
            if (Value != null)
            {
                return propertyValue != null && propertyValue.ToString() == Value.ToString();
            }
            return propertyValue != null && !string.IsNullOrEmpty(propertyValue.ToString());
        }
    }
}
