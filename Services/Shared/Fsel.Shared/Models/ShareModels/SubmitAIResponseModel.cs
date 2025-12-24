// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Models.ShareModels
{
    using Fsel.Shared.Enums;

    public class SubmitAIResponseModel
    {
        public string? GradingAlFeedback { get; set; }
        public Guid? ClassForumResultId { get; set; }
        public EnumSubmissionCount EnumSubmissionCount { get; set; }

        public string? PronunciationAlFeedback { get; set; }

        public double? PronunciationScore { get; set; }
    }

    public class SubmitMockTestResponseModel
    {
        public string? CriteriaName { get; set; }
        public int? DisplayOrder { get; set; }
        public Guid? MockTestResultId { get; set; }
        public string? GradingAlFeedBack { get; set; }
    }

    public class SubmitTestResponseModel
    {
        public string? CriteriaName { get; set; }
        public int? DisplayOrder { get; set; }
        public Guid? TestResultId { get; set; }
        public string? GradingAlFeedBack { get; set; }
    }

    public class SubmitExamPracticeResponseModel
    {
        public string? CriteriaName { get; set; }
        public int? DisplayOrder { get; set; }
        public string? GradingAlFeedBack { get; set; }
        public Guid? ExamPracticeResultId { get; set; }
    }

    public class SubmitAiSpeakingResponseModel
    {
        public string? CriteriaName { get; set; }

        public long BandScore { get; set; }

        public string? BandDescriptionText { get; set; }

        public Guid MockTestResultId { get; set; }
    }

    public class SubmitTestAiSpeakingResponseModel
    {
        public string? CriteriaName { get; set; }
        public double BandScore { get; set; }
        public string? BandDescriptionText { get; set; }
        public Guid TestResultId { get; set; }
    }

    public class SubmitExamPracticeAiSpeakingResponseModel
    {
        public string? CriteriaName { get; set; }
        public int? DisplayOrder { get; set; }
        public double BandScore { get; set; }
        public Guid ExamPracticeResultId { get; set; }
        public IList<ExamPracticeAIGradingLanguageModel> ExamPracticeAIGradingLanguages { get; set; } = new List<ExamPracticeAIGradingLanguageModel>();
    }

    public class ExamPracticeAIGradingLanguageModel
    {
        public string? Language { get; set; }
        public IList<ExamPracticeAIGradingModel>? ExamPracticeAIGradings { get; set; }
    }

    public class ExamPracticeAIGradingModel
    {
        public string? ErrorMessage { get; set; }
        public string? BandScore { get; set; }
        public string? BandDescriptorText { get; set; }
        public string? Explanation { get; set; }
        public string? SuggestionsForImprovement { get; set; }
    }

    public class SpeakingAIEvaluationModel
    {
        public Guid MockTestResultId { get; set; }
        public Guid SectionGroupId { get; set; }
    }

    public class SpeakingExamPracticeAIEvaluationModel
    {
        public Guid ExamPracticeResultId { get; set; }
        public Guid ExamPracticeSectionId { get; set; }
    }
}
