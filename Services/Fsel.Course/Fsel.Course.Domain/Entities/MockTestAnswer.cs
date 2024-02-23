// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;

    public class MockTestAnswer : BaseAnswer
    {
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

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public override string? AnswerStr
        {
            get { return _answerStr; }
            set
            {
                _answerStr = value;
                TimeCount = MediaHelper.GetMediaDurationAsync(value);
                WordCount = StringHelper.CountWords(value);
            }
        }

        private int? _timeCount;

        public int? TimeCount
        {
            get { return _timeCount == null ? MediaHelper.GetMediaDurationAsync(AnswerStr) : _timeCount; }
            set { _timeCount = value; }
        }

        private int? _wordCount;

        public int? WordCount
        {
            get { return _wordCount == null ? StringHelper.CountWords(AnswerStr) : _wordCount; }
            set { _wordCount = value; }
        }
        public MockTestResult? MockTestResult { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid MockTestResultId { get; set; }

        public SectionGroupResult? SectionGroupResult { get; set; }
        public Guid? SectionGroupResultId { get; set; }

        public string? GradingAlFeedback { get; set; }
    }
}
