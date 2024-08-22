// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Text.RegularExpressions;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities.QuestionTypeConfigs.Questions;
    using Fsel.Course.Domain.Entities.QuestionTypeConfigs.Questions.V1i1;
    using Fsel.Shared.Enums;

    public class QuestionTypeConverter
    {
        public (object?, int) QuestionTypeConverterObject(object? config, EnumQuestionType type, bool isShowCorrectTotal = false, bool isDisableAnswers = false)
        {
            int totalCorrect = default;
            object? result;
            switch (type)
            {
                case EnumQuestionType.Multichoice:
                case EnumQuestionType.Dropdown:
                    var multichoice = config.Deserialize<MultipleChoiceQuestion>();
                    result = isDisableAnswers ? ClearAnswers(multichoice) : multichoice;
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect(multichoice) : default;
                    break;

                case EnumQuestionType.Checklist:
                    var checklist = config.Deserialize<MultipleChoiceQuestion>();
                    result = isDisableAnswers ? ClearAnswers(checklist) : checklist;
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect(checklist) : default;
                    break;

                case EnumQuestionType.Listing:
                    result = config.Deserialize<ListingQuestion>();
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect(result) : default;
                    break;

                case EnumQuestionType.MatchingType1:
                case EnumQuestionType.MatchingType2:
                case EnumQuestionType.DragAndDropPicture:
                    var matchingTypeQuestion = config.Deserialize<MatchingTypeQuestion>();
                    result = isDisableAnswers ? ClearAnswers(matchingTypeQuestion) : matchingTypeQuestion;
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect(matchingTypeQuestion) : default;
                    break;

                case EnumQuestionType.ShortAnswerWordBase:
                    var shortAnswerQuestionWordBaseQuestion = config.Deserialize<ShortAnswerQuestionWordBaseQuestion>();
                    result = isDisableAnswers ? ClearAnswers(shortAnswerQuestionWordBaseQuestion) : shortAnswerQuestionWordBaseQuestion;
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect(result) : default;
                    break;

                case EnumQuestionType.ShortAnswerWordCount:
                    result = config.Deserialize<ShortAnswerQuestionWordCountBaseQuestion>();
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect(result) : default;
                    break;

                case EnumQuestionType.GapFillScoreByQuestion:
                    var gapFillQuestion = config.Deserialize<GapFillQuestion>();
                    result = isDisableAnswers ? ClearAnswers(gapFillQuestion) : gapFillQuestion;
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect(gapFillQuestion) : default;
                    break;

                case EnumQuestionType.GapFillWordBankScoreByQuestion:
                    var gapFillWordBankScoreQuestion = config.Deserialize<GapFillQuestion>();
                    result = isDisableAnswers ? ClearAnswers(gapFillWordBankScoreQuestion) : gapFillWordBankScoreQuestion;
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect(gapFillWordBankScoreQuestion) : default;
                    break;

                case EnumQuestionType.GapFillWordBankScoreByGap:
                    var gapFillWordBankScoreByGap = config.Deserialize<GapFillQuestion>();
                    result = isDisableAnswers ? ClearAnswers(gapFillWordBankScoreByGap) : gapFillWordBankScoreByGap;
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect(gapFillWordBankScoreByGap, true) : default;
                    break;

                case EnumQuestionType.GapFillScoreByGap:
                    var gapFillQuestionByGap = config.Deserialize<GapFillQuestion>();
                    result = isDisableAnswers ? ClearAnswers(gapFillQuestionByGap) : gapFillQuestionByGap;
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect(gapFillQuestionByGap) : default;
                    break;

                case EnumQuestionType.DragAndDropSentenceOrder:
                    var dragAndDropSentenceOrderQuestion = config.Deserialize<DragAndDropSentenceOrderQuestion>();
                    result = isDisableAnswers ? ClearAnswers(dragAndDropSentenceOrderQuestion) : dragAndDropSentenceOrderQuestion;
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect(dragAndDropSentenceOrderQuestion) : default;
                    break;

                case EnumQuestionType.DragAndDropListSentenceOrder:
                    var dragAndDropList = config.Deserialize<DragAndDropListSentenceOrderQuestion>();
                    if (dragAndDropList != null)
                    {
                        dragAndDropList.Contents = dragAndDropList.Contents?.Select((x, index) => { x.Id = ++index; return x; }).ToList();
                    }
                    result = isDisableAnswers ? ClearAnswers(dragAndDropList) : dragAndDropList;
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect(dragAndDropList) : default;
                    break;

                case EnumQuestionType.MultipleOptionSentenceCompletion:
                    var multipleOption = config.Deserialize<MultipleOptionSentenceCompletionQuestion>();
                    result = isDisableAnswers ? ClearAnswers(multipleOption) : multipleOption;
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect(multipleOption) : default;
                    break;

                case EnumQuestionType.ExercisePreparation:
                    result = config.Deserialize<ExercisePreparationQuestion>();
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect(result) : default;
                    break;

                // Dạng câu hỏi mới
                case EnumQuestionType.MatchingParagraphInfo:
                    var matchingParagraphInfo = HandleQuestion(config.Deserialize<MatchingTaskQuestion>());
                    result = isDisableAnswers ? ClearAnswers(matchingParagraphInfo) : matchingParagraphInfo;
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect(matchingParagraphInfo) : default;
                    break;

                case EnumQuestionType.MatchingHeading:
                    var matchingHeading = HandleQuestion(config.Deserialize<MatchingTaskQuestion>());
                    result = isDisableAnswers ? ClearAnswers(matchingHeading) : matchingHeading;
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect(matchingHeading) : default;
                    break;

                case EnumQuestionType.YesNoNotGivenDropDown:
                    var yesNoNotGivenDropDown = HandleQuestion(config.Deserialize<MatchingTaskQuestion>());
                    result = isDisableAnswers ? ClearAnswers(yesNoNotGivenDropDown) : yesNoNotGivenDropDown;
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect(yesNoNotGivenDropDown) : default;
                    break;

                case EnumQuestionType.TrueFalseNotGivenDropDown:
                    var trueFalseNotGivenDropDown = HandleQuestion(config.Deserialize<MatchingTaskQuestion>());
                    result = isDisableAnswers ? ClearAnswers(trueFalseNotGivenDropDown) : trueFalseNotGivenDropDown;
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect(trueFalseNotGivenDropDown) : default;
                    break;

                case EnumQuestionType.MapLabelingDropDown:
                    var mapLabelingDropDown = HandleQuestion(config.Deserialize<MatchingTaskQuestion>());
                    result = isDisableAnswers ? ClearAnswers(mapLabelingDropDown) : mapLabelingDropDown;
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect(mapLabelingDropDown) : default;
                    break;

                case EnumQuestionType.SummaryCompletionDropDown:
                    var summaryCompletionDropDown = HandleQuestion(config.Deserialize<MatchingTaskQuestion>());
                    result = isDisableAnswers ? ClearAnswers(summaryCompletionDropDown) : summaryCompletionDropDown;
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect(summaryCompletionDropDown) : default;
                    break;

                case EnumQuestionType.MultichoiceV1:
                    var multichoiceV1 = config.Deserialize<MultipleChoiceQuestionV1>();
                    result = isDisableAnswers ? ClearAnswers(multichoiceV1) : multichoiceV1;
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect(multichoiceV1) : default;
                    break;

                case EnumQuestionType.CheckListV1:
                    var checkList = HandleQuestion(config.Deserialize<CheckListQuestionV1>());
                    result = isDisableAnswers ? ClearAnswers(checkList) : checkList;
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect(checkList) : default;
                    break;

                case EnumQuestionType.SummaryCompletionGapFill:
                    var summaryCompletionGapFill = HandleQuestion(config.Deserialize<CheckListQuestionV1>());
                    result = isDisableAnswers ? ClearAnswers(summaryCompletionGapFill) : summaryCompletionGapFill;
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect(summaryCompletionGapFill) : default;
                    break;

                case EnumQuestionType.CompletionDiagrams:
                case EnumQuestionType.FlowChartCompletion:
                    var completionDiagrams = HandleQuestion(config.Deserialize<CheckListQuestionV1>());
                    result = isDisableAnswers ? ClearAnswers(completionDiagrams) : completionDiagrams;
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect(completionDiagrams) : default;
                    break;

                //var flowChartCompletion = HandleQuestion(config.Deserialize<FlowChartCompletionQuestion>());
                //result = isDisableAnswers ? ClearAnswers(flowChartCompletion) : flowChartCompletion;
                //totalCorrect = isShowCorrectTotal ? GetTotalCorrect(flowChartCompletion) : default;
                //break;

                case EnumQuestionType.TableCompletion:
                    var tableCompletion = HandleQuestion(config.Deserialize<TableCompletionQuestion>());
                    result = isDisableAnswers ? ClearAnswers(tableCompletion) : tableCompletion;
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect(tableCompletion) : default;
                    break;

                default:
                    throw new ArgumentException("Invalid question type");
            }
            return (result, totalCorrect);
        }

        public bool ValidateQuestion(object? config, EnumQuestionType questionType)
        {
            var isError = false;
            switch (questionType)
            {
                case EnumQuestionType.MultichoiceV1:
                    var multichoiceV1 = config.Deserialize<MultipleChoiceQuestionV1>();
                    isError = ValidateMultichoiceV1(multichoiceV1);
                    break;

                case EnumQuestionType.YesNoNotGivenDropDown:
                    var yesNoNotGivenDropDown = config.Deserialize<MatchingTaskQuestion>();
                    isError = ValidateMatchingTask(yesNoNotGivenDropDown);
                    break;

                case EnumQuestionType.TrueFalseNotGivenDropDown:
                    var trueFalseNotGivenDropDown = config.Deserialize<MatchingTaskQuestion>();
                    isError = ValidateMatchingTask(trueFalseNotGivenDropDown);
                    break;

                case EnumQuestionType.MapLabelingDropDown:
                    var mapLabelingDropDown = config.Deserialize<MatchingTaskQuestion>();
                    isError = ValidateMatchingTask(mapLabelingDropDown);
                    break;

                case EnumQuestionType.SummaryCompletionDropDown:
                    var summaryCompletionDropDown = config.Deserialize<MatchingTaskQuestion>();
                    isError = ValidateMatchingTask(summaryCompletionDropDown) || string.IsNullOrEmpty(summaryCompletionDropDown?.Content);
                    break;

                case EnumQuestionType.MatchingParagraphInfo:
                    var matchingTask = config.Deserialize<MatchingTaskQuestion>();
                    isError = ValidateMatchingTask(matchingTask);
                    break;

                case EnumQuestionType.MatchingHeading:
                    var matchingHeading = config.Deserialize<MatchingTaskQuestion>();
                    isError = ValidateMatchingTask(matchingHeading);
                    break;

                case EnumQuestionType.CheckListV1:
                    var checkList = config.Deserialize<CheckListQuestionV1>();
                    isError = ValidateCheckList(checkList);
                    break;

                case EnumQuestionType.SummaryCompletionGapFill:
                    var summaryCompletionGapFill = config.Deserialize<CheckListQuestionV1>();
                    isError = ValidateCheckList(summaryCompletionGapFill) || string.IsNullOrEmpty(summaryCompletionGapFill?.Content);
                    break;

                case EnumQuestionType.CompletionDiagrams:
                    var completionDiagrams = config.Deserialize<CheckListQuestionV1>();
                    isError = ValidateCheckList(completionDiagrams);
                    break;

                case EnumQuestionType.TableCompletion:
                    var tableCompletion = config.Deserialize<TableCompletionQuestion>();
                    isError = ValidateTableCompletion(tableCompletion);
                    break;

                case EnumQuestionType.FlowChartCompletion:
                    var flowChartCompletion = config.Deserialize<CheckListQuestionV1>();
                    isError = ValidateCheckList(flowChartCompletion);
                    break;

                default:
                    break;
            }
            return isError;
        }

        private static bool ValidateTableCompletion(TableCompletionQuestion? data)
        {
            var isError = true;
            if (data == null || !data.AnswerTables.Any())
            {
                return isError;
            }
            foreach (var item in data.AnswerTables)
            {
                if (string.IsNullOrEmpty(item.Content))
                {
                    return isError;
                }
            }
            if (HasInvalidKeysOrContent(data.Rows))
            {
                return isError;
            }
            return false;
        }

        private static bool ValidatFlowChartCompletion(FlowChartCompletionQuestion? data)
        {
            if (data == null || !data.Answers.Any())
            {
                return true;
            }
            if (HasInvalidKeysOrContent(data.Answers))
            {
                return true;
            }
            return false;
        }

        private static bool ValidateCheckList(CheckListQuestionV1? data)
        {
            if (data == null || !data.Answers.Any())
            {
                return true;
            }
            if (HasInvalidKeysOrContent(data.Answers))
            {
                return true;
            }
            return false;
        }

        private static bool ValidateMultichoiceV1(MultipleChoiceQuestionV1? data)
        {
            var isError = true;
            if (data == null || !data.Answers.Any())
            {
                return isError;
            }

            foreach (var item in data.Answers)
            {
                if (string.IsNullOrEmpty(item.Content) || !item.Answers.Any())
                {
                    return isError;
                }
                if (HasInvalidKeysOrContent(item.Answers))
                {
                    return isError;
                }
            }
            return false;
        }

        private static bool ValidateMatchingTask(MatchingTaskQuestion? data)
        {
            if (data == null || !data.Answers.Any() || !data.Placeholders.Any())
            {
                return true;
            }
            if (HasInvalidKeysOrContent(data.Answers) || HasInvalidKeysOrContent(data.Placeholders))
            {
                return true;
            }
            if (data.Placeholders.GroupBy(x => x.Key).Select(x => x.Count()).Any(x => x > 1))
            {
                return true;
            }
            return false;
        }

        private static bool HasInvalidKeysOrContent(IEnumerable<dynamic> items)
        {
            // Kiểm tra sự đồng nhất của Key
            bool hasEmptyKey = items.Any(x => string.IsNullOrEmpty((string)x.Key));
            bool hasNonEmptyKey = items.Any(x => !string.IsNullOrEmpty((string)x.Key));
            if (hasEmptyKey && hasNonEmptyKey)
            {
                return true;
            }
            // Kiểm tra sự đồng nhất của Content
            bool hasEmptyContent = items.Any(x => string.IsNullOrEmpty((string)x.Content));
            bool hasNonEmptyContent = items.Any(x => !string.IsNullOrEmpty((string)x.Content));
            if (hasEmptyContent && hasNonEmptyContent)
            {
                return true;
            }
            return false;
        }

        private static TableCompletionQuestion? HandleQuestion(TableCompletionQuestion? data)
        {
            if (data == null || data.Rows == null || !data.Rows.Any())
            {
                return data;
            }
            foreach (var item in data.Rows)
            {
                if (string.IsNullOrEmpty(item.Content))
                {
                    continue;
                }
                MatchCollection matches = Regex.Matches(item.Content, @"\{(.*?)\}");
                Dictionary<string, Guid?> replacements = new Dictionary<string, Guid?>();

                foreach (Match match in matches)
                {
                    string id = match.Groups[1].Value;
                    if (Guid.TryParse(id, out _))
                    {
                        continue;
                    }
                    var config = new AnswerTable
                    {
                        RowId = item.Id ?? Guid.NewGuid(),
                        Content = match.Groups[1].Value
                    };
                    data.AnswerTables.Add(config);
                    replacements[id] = config.Id;
                }

                item.Content = ReplacePlaceholders(item.Content, replacements);
            }

            return data;
        }

        private static FlowChartCompletionQuestion? HandleQuestion(FlowChartCompletionQuestion? data)
        {
            if (data == null || data.Contents == null || !data.Contents.Any())
            {
                return data;
            }
            foreach (var item in data.Contents)
            {
                foreach (var item2 in item.Contents)
                {
                    if (string.IsNullOrEmpty(item2.Content))
                    {
                        continue;
                    }
                    MatchCollection matches = Regex.Matches(item2.Content, @"\{(.*?)\}");
                    Dictionary<string, Guid?> replacements = new Dictionary<string, Guid?>();
                    if (!matches.Any())
                    {
                        continue;
                    }
                    foreach (Match match in matches)
                    {
                        string id = match.Groups[1].Value;
                        if (!Guid.TryParse(id, out _))
                        {
                            var config = new ConfigQuestionV1
                            {
                                Id = Guid.NewGuid(),
                                Content = match.Groups[1].Value
                            };
                            data.Answers.Add(config);
                            replacements[id] = config.Id;
                        }
                        else
                        {
                            replacements[id] = new Guid(id);
                        }
                    }

                    item2.Content = ReplacePlaceholders(item2.Content, replacements);
                }
            }

            return data;
        }

        private static dynamic? HandleQuestion(dynamic? data, dynamic? dataList = null)
        {
            if (dataList is IList list)
            {
                // Nếu data là danh sách, đệ quy xử lý từng phần tử
                foreach (dynamic item in list.OfType<dynamic>())
                {
                    HandleQuestion(item, data);
                }
                return data; // Trả về danh sách đã được xử lý
            }
            if (data == null)
            {
                return null;
            }
            var content = data.Content;
            if (string.IsNullOrEmpty(content))
            {
                return data;
            }

            MatchCollection matches = Regex.Matches(content, @"\{(.*?)\}");
            if (!matches.Any())
            {
                return data;
            }

            // Nếu data không phải là danh sách, xử lý thay thế chỗ trống
            Dictionary<string, Guid?> replacements = new Dictionary<string, Guid?>();

            foreach (Match match in matches)
            {
                string id = match.Groups[1].Value;
                if (!Guid.TryParse(id, out _))
                {
                    var config = new ConfigQuestionV1
                    {
                        Id = Guid.NewGuid(),
                        Key = id
                    };

                    // Nếu dataOld có danh sách các câu trả lời, thêm câu trả lời mới vào danh sách đó
                    if (data.Answers != null)
                    {
                        data.Answers.Add(config);
                    }
                    else
                    {
                        // Nếu dataOld không có danh sách Answers, khởi tạo một danh sách mới
                        data.Answers = new List<ConfigQuestionV1> { config };
                    }

                    replacements[id] = config.Id;
                }
                else
                {
                    replacements[id] = new Guid(id);
                }
            }

            data.Content = ReplacePlaceholders(content, replacements);
            return data;
        }

        private static string? ReplacePlaceholders(string? content, Dictionary<string, Guid?> replacements)
        {
            foreach (var pair in replacements)
            {
                if (!Guid.TryParse(pair.Key, out _))
                {
                    content = ReplaceFirst(content, "{" + pair.Key + "}", "{" + pair.Value + "}");
                }
            }
            return content;
        }

        public static string? ReplaceFirst(string? str, string? term, string? replace)
        {
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(term))
            {
                return str;
            }
            int position = str.IndexOf(term, StringComparison.CurrentCulture);
            if (position < 0)
            {
                return str;
            }

            str = string.Concat(str.AsSpan(0, position), replace, str.AsSpan(position + term.Length));
            return str;
        }

        private static object? ClearAnswers<T>(T? data) where T : class
        {
            switch (data)
            {
                case MatchingTaskQuestion matchingTask:
                    if (matchingTask.Answers != null)
                    {
                        foreach (var item in matchingTask.Answers)
                        {
                            item.Key = null;
                        }
                    }
                    break;

                case TableCompletionQuestion tableCompletion:
                    tableCompletion.AnswerTables = new List<AnswerTable>();
                    break;

                case FlowChartCompletionQuestion flowChart:
                    flowChart.Answers = new List<ConfigAnswerV1>();
                    break;

                case CheckListQuestionV1 checkList:
                    if (checkList.Answers != null)
                    {
                        foreach (var item in checkList.Answers)
                        {
                            if (item.IsCorrect.HasValue)
                            {
                                item.IsCorrect = null;
                            }
                            else
                            {
                                item.Content = null;
                            }
                        }
                    }
                    break;

                case MultipleChoiceQuestionV1 multipleChoiceV1:
                    if (multipleChoiceV1.Answers != null)
                    {
                        foreach (var item in multipleChoiceV1.Answers.SelectMany(x => x.Answers))
                        {
                            item.IsCorrect = null;
                        }
                    }
                    break;

                case MultipleOptionSentenceCompletionQuestion multipleOption:
                    if (multipleOption.Contents != null)
                    {
                        foreach (var item in multipleOption.Contents)
                        {
                            item?.Answers.ForEach(x => x.IsCorrect = null);
                        }
                    }
                    break;

                case ShortAnswerQuestionWordBaseQuestion shortAnswer:
                    shortAnswer.Content?.Clear();
                    break;

                case MultipleChoiceQuestion multipleChoice:
                    if (multipleChoice.Contents != null)
                    {
                        foreach (var item in multipleChoice.Contents)
                        {
                            item.IsCorrect = default;
                        }
                    }
                    break;

                case DragAndDropSentenceOrderQuestion dragAndDrop:
                    if (dragAndDrop.Contents != null)
                    {
                        foreach (var item in dragAndDrop.Contents)
                        {
                            item.Words = GenerateRandomLoop(item.Words);
                        }
                    }
                    break;

                case DragAndDropListSentenceOrderQuestion dragAndDropList:
                    return GenerateRandomLoop(dragAndDropList.Contents);

                case MatchingTypeQuestion matchingType:
                    matchingType.Link?.Clear();
                    break;

                case GapFillQuestion gapFill:
                    if (gapFill.Contents != null)
                    {
                        foreach (var item in gapFill.Contents)
                        {
                            item.Words = GenerateRandomLoop(item.Words);
                        }
                    }
                    break;
            }
            return data;
        }

        private static int GetTotalCorrect<T>(T? data, bool calculateByGap = false) where T : class
        {
            switch (data)
            {
                case MultipleChoiceQuestion multipleChoice:
                    return multipleChoice.Contents?.Count(x => x.IsCorrect.HasValue && x.IsCorrect.Value) ?? default;

                case MultipleOptionSentenceCompletionQuestion multipleOption:
                    return multipleOption.Contents?.Count ?? default;

                case MatchingTypeQuestion matchingType:
                    return matchingType.Link?.Count ?? default;

                case GapFillQuestion gapFill when !calculateByGap:
                    return gapFill.Contents?.Count ?? default;

                case GapFillQuestion gapFill when calculateByGap:
                    return gapFill.Contents?.Sum(x => x.Words?.Count ?? default) ?? default;

                case DragAndDropSentenceOrderQuestion dragAndDrop:
                    return dragAndDrop.Contents?.Count ?? default;

                // V1
                case MultipleChoiceQuestionV1 multipleChoiceV1:
                    return multipleChoiceV1.Answers?
                        .Where(x => x.Answers != null && x.Answers.Any())
                        .SelectMany(x => x.Answers!)
                        .Count(x => x.IsCorrect.HasValue && x.IsCorrect.Value) ?? default;

                case CheckListQuestionV1 checkList:
                    if (checkList.Answers != null)
                    {
                        return checkList.Answers.Any(x => x.IsCorrect.HasValue)
                            ? checkList.Answers.Count(x => x.IsCorrect.HasValue && x.IsCorrect.Value)
                            : checkList.Answers.Count;
                    }
                    return default;

                case MatchingTaskQuestion matchingTask:
                    return matchingTask.Answers?.Count ?? default;

                case TableCompletionQuestion tableCompletion:
                    return tableCompletion.AnswerTables?.Count ?? default;

                case FlowChartCompletionQuestion flowChart:
                    return flowChart.Answers?.Count ?? default;

                default:
                    return 1;
            }
        }

        private static IList<T>? GenerateRandomLoop<T>(IList<T>? datas)
        {
            var rand = new Random();
            if (datas != null)
            {
                return datas.OrderBy(_ => rand.Next()).ToList();
            }
            return datas;
        }
    }
}
