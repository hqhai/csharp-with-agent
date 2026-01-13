// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common.VadilatorHelper
{
    using System.Text;

    /// <summary>
    /// Helper for validation with Strategy Pattern
    /// </summary>
    public static class ValidationHelper
    {
        /// <summary>
        /// Collect validation errors using strategy pattern
        /// </summary>
        public static StringBuilder CollectValidationErrors(params (bool IsValid, string FieldName)[] validations)
        {
            var errorFields = new StringBuilder();

            foreach (var (isValid, fieldName) in validations)
            {
                if (!isValid)
                {
                    if (errorFields.Length > 0)
                    {
                        errorFields.Append(", ");
                    }
                    errorFields.Append(fieldName);
                }
            }

            return errorFields;
        }
    }
}
