namespace Fsel.System.Domain.Models.CommandModels.DailyQuiz
{
    using global::System.Text.Json;
    using OfficeOpenXml.Attributes;

    public class ImportQuestionModel
    {
        [EpplusTableColumn(Header = "Category")]
        public string? Category { get; set; }

        [EpplusTableColumn(Header = "Subcategory")]
        public string? Subcategory { get; set; }

        [EpplusTableColumn(Header = "Question Text (English)")]
        public string? QuestionEnglish { get; set; }

        [EpplusTableColumn(Header = "Question Text (Vietnamese)")]
        public string? QuestionVietnamese { get; set; }

        [EpplusTableColumn(Header = "Option A")]
        public string? OptionA { get; set; }

        public ImportQuestionTranslationModel? AnswerA
        {
            get
            {
                return OptionA != null
                    ? JsonSerializer.Deserialize<ImportQuestionTranslationModel>(OptionA)
                    : null;
            }
        }

        [EpplusTableColumn(Header = "Option B")]
        public string? OptionB { get; set; }

        public ImportQuestionTranslationModel? AnswerB
        {
            get
            {
                return OptionB != null
                    ? JsonSerializer.Deserialize<ImportQuestionTranslationModel>(OptionB)
                    : null;
            }
        }

        [EpplusTableColumn(Header = "Option C")]
        public string? OptionC { get; set; }

        public ImportQuestionTranslationModel? AnswerC
        {
            get
            {
                return OptionC != null
                    ? JsonSerializer.Deserialize<ImportQuestionTranslationModel>(OptionC)
                    : null;
            }
        }

        [EpplusTableColumn(Header = "Correct Answer")]
        public string? CorrectAnswer { get; set; }

        [EpplusTableColumn(Header = "Explanation")]
        public string? Explanation { get; set; }

        public ImportQuestionTranslationModel? ExplanationModel
        {
            get
            {
                return Explanation != null
                    ? JsonSerializer.Deserialize<ImportQuestionTranslationModel>(Explanation)
                    : null;
            }
        }
    }

    public class ImportQuestionTranslationModel
    {
        public string? vi { get; set; }

        public string? en { get; set; }
    }
}
