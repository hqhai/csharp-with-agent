// Copyright (c) Atlantic. All rights reserved.

using System.ComponentModel.DataAnnotations.Schema;

namespace Fsel.Common.ActionResults
{
    [NotMapped]
    public class ErrorResult
    {
        public string? ErrorCode { get; set; }

        public IList<Error> Errors { get; set; }

        public ErrorResult()
        {
            Errors = new List<Error>();
        }
    }

    [NotMapped]
    public class Error
    {
        public Error()
        {
            ErrorValues = new List<object>();
            ExactValues = new List<object>();
        }

        public Error(string? fieldName)
        {
            FieldName = fieldName;
        }

        public Error(object? errorValue)
        {
            if (errorValue != null)
            {
                ErrorValues = new List<object>() { errorValue };
            }
        }

        public Error(string? fieldName, object? errorValue)
        {
            FieldName = fieldName;
            if (errorValue != null)
            {
                ErrorValues = new List<object>() { errorValue };
            }
        }

        public Error(string? fieldName, IList<object>? errorValues)
        {
            FieldName = fieldName;
            ErrorValues = errorValues;
        }

        public Error(string? fieldName, IList<object>? errorValues, IList<object>? exactValues)
        {
            FieldName = fieldName;
            ErrorValues = errorValues;
            ExactValues = exactValues;
        }

        public string? FieldName { get; set; }
        public IList<object>? ErrorValues { get; set; }
        public IList<object>? ExactValues { get; set; }
    }
}
