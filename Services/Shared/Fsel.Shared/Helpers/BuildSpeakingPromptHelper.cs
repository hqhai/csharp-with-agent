// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Helpers
{
    using System.Globalization;
    using System.Text;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;

    public static class BuildSpeakingPromptHelper
    {
        public static string CustomAnswerConfigToSendGPT(IList<string> questions, IList<string> answers, EnumMockTestScoreCriteria criteria)
        {
            StringBuilder sb = new StringBuilder();
            string defaultConfigByCriteria = GetConfigByType(criteria, false);

            // Generate questions
            sb.AppendLine("Speaking Test Questions:");
            for (int i = 0; i < questions.Count; i++)
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, "\"question{0}\": \"{1}\"\n", NumberToWords(i + 1), questions[i]);
            }

            sb.AppendLine();

            // Generate answers
            sb.AppendLine("Student Submission:");
            for (int i = 0; i < answers.Count; i++)
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, "\"answer{0}\": \"{1}\"\n", NumberToWords(i + 1), answers[i]);
            }
            sb.AppendLine();

            string result = sb.ToString();

            result = string.Concat(result, " ", defaultConfigByCriteria);

            return result;
        }

        public static string CustomAnswerConfigToSendGPT(IList<string> questions, IList<string> answers, EnumTestScoreCriteria criteria, string? userRole)
        {
            StringBuilder sb = new StringBuilder();
            string defaultConfigByCriteria = !string.IsNullOrEmpty(userRole) ? userRole : GetConfigByType(criteria, false);

            // Generate questions
            sb.AppendLine("Speaking Test Questions:");
            for (int i = 0; i < questions.Count; i++)
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, "\"question{0}\": \"{1}\"\n", NumberToWords(i + 1), questions[i]);
            }

            sb.AppendLine();

            // Generate answers
            sb.AppendLine("Student Submission:");
            for (int i = 0; i < answers.Count; i++)
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, "\"answer{0}\": \"{1}\"\n", NumberToWords(i + 1), answers[i]);
            }
            sb.AppendLine();

            string result = sb.ToString();

            result = string.Concat(result, " ", defaultConfigByCriteria);

            return result;
        }

        public static string GetConfigByType(EnumMockTestScoreCriteria criteria, bool isUserConfig)
        {
            string result = string.Empty;
            switch (criteria)
            {
                case EnumMockTestScoreCriteria.GrammaticalRangeAndAccuracy:
                    result = isUserConfig ? File.ReadAllText(ResourceSettings.SpeakingGrammarRole) : File.ReadAllText(ResourceSettings.SpeakingGrammar);
                    break;

                case EnumMockTestScoreCriteria.LexicalResource:
                    result = isUserConfig ? File.ReadAllText(ResourceSettings.SpeakingLexicalRole) : File.ReadAllText(ResourceSettings.SpeakingLexical);
                    break;

                case EnumMockTestScoreCriteria.FluencyAndCoherence:
                    result = isUserConfig ? File.ReadAllText(ResourceSettings.SpeakingFluencyRole) : File.ReadAllText(ResourceSettings.SpeakingFluency);

                    break;
            }

            return result;
        }

        /// <summary>
        /// Lấy config của AI Speaking theo tiêu chí
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="isUserConfig"></param>
        /// <returns></returns>
        public static string GetConfigByType(EnumTestScoreCriteria criteria, bool isUserConfig)
        {
            string result = string.Empty;
            switch (criteria)
            {
                case EnumTestScoreCriteria.GrammaticalRangeAndAccuracy:
                    result = isUserConfig ? File.ReadAllText(ResourceSettings.SpeakingGrammarRole) : File.ReadAllText(ResourceSettings.SpeakingGrammar);
                    break;

                case EnumTestScoreCriteria.LexicalResource:
                    result = isUserConfig ? File.ReadAllText(ResourceSettings.SpeakingLexicalRole) : File.ReadAllText(ResourceSettings.SpeakingLexical);
                    break;

                case EnumTestScoreCriteria.FluencyAndCoherence:
                    result = isUserConfig ? File.ReadAllText(ResourceSettings.SpeakingFluencyRole) : File.ReadAllText(ResourceSettings.SpeakingFluency);

                    break;
            }

            return result;
        }

        /// <summary>
        /// Chuyển đổi số thành chữ
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static string NumberToWords(int number)
        {
            if (number == 0)
            {
                return "Zero";
            }
            if (number < 0)
            {
                return "Minus" + NumberToWords(Math.Abs(number));
            }

            string words = "";

            if ((number / 1000000) > 0)
            {
                words += NumberToWords(number / 1000000) + " Million ";
                number %= 1000000;
            }
            if ((number / 1000) > 0)
            {
                words += NumberToWords(number / 1000) + " Thousand ";
                number %= 1000;
            }
            if ((number / 100) > 0)
            {
                words += NumberToWords(number / 100) + " Hundred ";
                number %= 100;
            }
            if (number > 0)
            {
                if (string.IsNullOrEmpty(words))
                {
                    words += "";
                }
                var unitsMap = new[] { "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
                var tensMap = new[] { "Zero", "Ten", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };

                if (number < 20)
                {
                    words += unitsMap[number];
                }
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0)
                    {
                        words += "-" + unitsMap[number % 10];
                    }
                }
            }

            return words;
        }
    }
}
