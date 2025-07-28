// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Common.ActionResults;
    using System.Reflection;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Shared.Helpers;
    using StringHelper = Shared.Helpers.StringHelper;
    using Newtonsoft.Json;

    public class MockTestAnswer : BaseAnswer
    {
        private const string answerStr = "{\"answers\":";
        public SectionQuestion? SectionQuestion { get; set; }
        public Guid? SectionQuestionId { get; set; }
        public SectionTimeCode? SectionTimeCode { get; set; }
        public Guid? SectionTimeCodeId { get; set; }
        public Section? Section { get; set; }
        public Guid? SectionId { get; set; }

        /// <summary>
        /// Câu trả lời
        /// </summary>
        private string? _answerStr;

        [MaxLength(11000, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public override string? AnswerStr
        {
            get { return _answerStr; }
            set
            {
                _answerStr = value;
                if (!string.IsNullOrEmpty(value) && !value.Contains(answerStr, StringComparison.InvariantCulture))
                {
                    TimeCount = MediaHelper.GetMediaDurationAsync(value);
                    if (TimeCount == null)
                    {
                        WordCount = StringHelper.CountWords(value);
                    }
                }
            }
        }

        [NotMapped]
        public override object? Answer
        {
            get { return ConvertHelper.Deserialize<object>(AnswerStr); }
            set { AnswerStr = value != null ? ConvertHelper.Serialize(value) : null; }
        }

        private int? _timeCount;

        public int? TimeCount
        {
            get { return _timeCount == null && !string.IsNullOrEmpty(AnswerStr) && !AnswerStr.Contains(answerStr, StringComparison.InvariantCulture) ? MediaHelper.GetMediaDurationAsync(AnswerStr) : _timeCount; }
            set { _timeCount = value; }
        }

        private int? _wordCount;

        public int? WordCount
        {
            get { return _wordCount == null && _timeCount == null && !string.IsNullOrEmpty(AnswerStr) && !AnswerStr.Contains(answerStr, StringComparison.InvariantCulture) ? StringHelper.CountWords(AnswerStr) : _wordCount; }
            set { _wordCount = value; }
        }

        public MockTestResult? MockTestResult { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid MockTestResultId { get; set; }

        public SectionGroupResult? SectionGroupResult { get; set; }
        public Guid? SectionGroupResultId { get; set; }

        public string? GradingAlFeedback { get; set; }

        public string? SpeechTextAnswer { get; set; }

        public double? PronunciationScore { get; set; }

        public int RetryTime { get; set; }

        public override bool IsValid()
        {
            ValidationContext validationContext = new ValidationContext(this, null, null);
            List<ValidationResult> list = new List<ValidationResult>();
            if (!Validator.TryValidateObject(this, validationContext, list, validateAllProperties: true))
            {
                foreach (ValidationResult item in list)
                {
                    ErrorResult errorResult = new ErrorResult
                    {
                        ErrorCode = item.ErrorMessage
                    };
                    foreach (string memberName in item.MemberNames)
                    {
                        PropertyInfo property = validationContext.ObjectType.GetProperty(memberName);
                        object obj = property?.GetValue(validationContext.ObjectInstance, null);
                        if (obj != null)
                        {
                            List<object> errorValues = new List<object> { obj };
                            List<object> exactValues = ((property?.GetCustomAttributesData())?.FirstOrDefault((CustomAttributeData x) => x.NamedArguments.Select((CustomAttributeNamedArgument n) => n.TypedValue.Value).Contains(item.ErrorMessage)))?.ConstructorArguments.Select((CustomAttributeTypedArgument x) => x.Value).Cast<object>().ToList();
                            errorResult.Errors.Add(new Error(memberName, errorValues, exactValues));
                        }
                        else
                        {
                            errorResult.Errors.Add(new Error(memberName));
                        }
                    }

                    AddErrorResults(errorResult);
                }

                _errorMessages.RemoveAll(err => err.ErrorCode == nameof(Required) && err.Errors.Any(e => e.FieldName == nameof(BaseAnswer.AnswerStr)));
            }

            return _errorMessages.Count == 0;
        }
    }
}
