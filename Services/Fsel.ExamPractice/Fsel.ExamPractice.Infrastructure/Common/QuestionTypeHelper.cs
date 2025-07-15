// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Infrastructure.Common
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Text.RegularExpressions;
    using Fsel.Common.Helpers;
    using Fsel.ExamPractice.Domain.Entities.QuestionTypeConfigs;
    using Fsel.ExamPractice.Domain.Entities.QuestionTypeConfigs.Questions;
    using Fsel.ExamPractice.Domain.Entities.QuestionTypeConfigs.Questions.V1i1;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;

    public static class QuestionTypeHelper
    {
        public static (object?, int) QuestionTypeConverterObject(object? config, EnumQuestionType type, bool isShowCorrectTotal = false, bool isDisableAnswers = false, bool isCreated = false)
        {
            int totalCorrect = default;
            object? result;
            switch (type)
            {
                case EnumQuestionType.Multichoice:
                case EnumQuestionType.Dropdown:
                    var multichoice = config.Deserialize<MultipleChoiceQuestion>();
                    result = isDisableAnswers ? ClearAnswers(multichoice) : multichoice;
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect(multichoice) : ValueSettings.ValueDefault;
                    break;

                case EnumQuestionType.Checklist:
                    var checklist = config.Deserialize<MultipleChoiceQuestion>();
                    result = isDisableAnswers ? ClearAnswers(checklist) : checklist;
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect(checklist) : ValueSettings.ValueDefault;
                    break;

                case EnumQuestionType.Listing:
                    result = config.Deserialize<ListingQuestion>();
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect(result) : ValueSettings.ValueDefault;
                    break;

                case EnumQuestionType.MatchingType1:
                case EnumQuestionType.MatchingType2:
                case EnumQuestionType.DragAndDropPicture:
                    var matchingTypeQuestion = config.Deserialize<MatchingTypeQuestion>();
                    result = isDisableAnswers ? ClearAnswers(matchingTypeQuestion) : matchingTypeQuestion;
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect(matchingTypeQuestion) : ValueSettings.ValueDefault;
                    break;

                case EnumQuestionType.ShortAnswerWordBase:
                    var shortAnswerQuestionWordBaseQuestion = config.Deserialize<ShortAnswerQuestionWordBaseQuestion>();
                    result = isDisableAnswers ? ClearAnswers(shortAnswerQuestionWordBaseQuestion) : shortAnswerQuestionWordBaseQuestion;
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect(result) : ValueSettings.ValueDefault;
                    break;

                case EnumQuestionType.ShortAnswerWordCount:
                    result = config.Deserialize<ShortAnswerQuestionWordCountBaseQuestion>();
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect(result) : ValueSettings.ValueDefault;
                    break;

                case EnumQuestionType.GapFillScoreByQuestion:
                    var gapFillQuestion = config.Deserialize<GapFillQuestion>();
                    result = isDisableAnswers ? ClearAnswers(gapFillQuestion) : gapFillQuestion;
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect(gapFillQuestion) : ValueSettings.ValueDefault;
                    break;

                case EnumQuestionType.GapFillWordBankScoreByQuestion:
                    var gapFillWordBankScoreQuestion = config.Deserialize<GapFillQuestion>();
                    result = isDisableAnswers ? ClearAnswers(gapFillWordBankScoreQuestion) : gapFillWordBankScoreQuestion;
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect(gapFillWordBankScoreQuestion) : ValueSettings.ValueDefault;
                    break;

                case EnumQuestionType.GapFillWordBankScoreByGap:
                    var gapFillWordBankScoreByGap = config.Deserialize<GapFillQuestion>();
                    result = isDisableAnswers ? ClearAnswers(gapFillWordBankScoreByGap) : gapFillWordBankScoreByGap;
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect(gapFillWordBankScoreByGap, true) : ValueSettings.ValueDefault;
                    break;

                case EnumQuestionType.GapFillScoreByGap:
                    var gapFillQuestionByGap = config.Deserialize<GapFillQuestion>();
                    result = isDisableAnswers ? ClearAnswers(gapFillQuestionByGap) : gapFillQuestionByGap;
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect(gapFillQuestionByGap, true) : ValueSettings.ValueDefault;
                    break;

                case EnumQuestionType.DragAndDropSentenceOrder:
                    var dragAndDropSentenceOrderQuestion = config.Deserialize<DragAndDropSentenceOrderQuestion>();
                    result = isDisableAnswers ? ClearAnswers(dragAndDropSentenceOrderQuestion) : dragAndDropSentenceOrderQuestion;
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect(dragAndDropSentenceOrderQuestion) : ValueSettings.ValueDefault;
                    break;

                case EnumQuestionType.DragAndDropListSentenceOrder:
                    var dragAndDropList = config.Deserialize<DragAndDropListSentenceOrderQuestion>();
                    if (dragAndDropList != null)
                    {
                        dragAndDropList.Contents = dragAndDropList.Contents?.Select((x, index) => { x.Id = ++index; return x; }).ToList();
                    }
                    result = isDisableAnswers ? ClearAnswers(dragAndDropList) : dragAndDropList;
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect(dragAndDropList) : ValueSettings.ValueDefault;
                    break;

                case EnumQuestionType.MultipleOptionSentenceCompletion:
                    var multipleOption = config.Deserialize<MultipleOptionSentenceCompletionQuestion>();
                    result = isDisableAnswers ? ClearAnswers(multipleOption) : multipleOption;
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect(multipleOption) : ValueSettings.ValueDefault;
                    break;

                case EnumQuestionType.ExercisePreparation:
                    result = config.Deserialize<ExercisePreparationQuestion>();
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect(result) : ValueSettings.ValueDefault;
                    break;

                // Dạng câu hỏi mới
                case EnumQuestionType.MatchingParagraphInfo:
                    var matchingParagraphInfo = HandleQuestion(config.Deserialize<MatchingTaskQuestion>(), isCreated);
                    result = isDisableAnswers ? ClearAnswers(matchingParagraphInfo) : matchingParagraphInfo;
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect(matchingParagraphInfo) : ValueSettings.ValueDefault;
                    break;

                case EnumQuestionType.MatchingHeading:
                    var matchingHeading = HandleQuestion(config.Deserialize<MatchingTaskQuestion>(), isCreated);
                    result = isDisableAnswers ? ClearAnswers(matchingHeading) : matchingHeading;
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect(matchingHeading) : ValueSettings.ValueDefault;
                    break;

                case EnumQuestionType.YesNoNotGivenDropDown:
                    var yesNoNotGivenDropDown = HandleQuestion(config.Deserialize<MatchingTaskQuestion>(), isCreated);
                    result = isDisableAnswers ? ClearAnswers(yesNoNotGivenDropDown) : yesNoNotGivenDropDown;
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect(yesNoNotGivenDropDown) : ValueSettings.ValueDefault;
                    break;

                case EnumQuestionType.TrueFalseNotGivenDropDown:
                    var trueFalseNotGivenDropDown = HandleQuestion(config.Deserialize<MatchingTaskQuestion>(), isCreated);
                    result = isDisableAnswers ? ClearAnswers(trueFalseNotGivenDropDown) : trueFalseNotGivenDropDown;
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect(trueFalseNotGivenDropDown) : ValueSettings.ValueDefault;
                    break;

                case EnumQuestionType.MapLabelingDropDown:
                    var mapLabelingDropDown = HandleQuestion(config.Deserialize<MatchingTaskQuestion>(), isCreated);
                    result = isDisableAnswers ? ClearAnswers(mapLabelingDropDown) : mapLabelingDropDown;
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect(mapLabelingDropDown) : ValueSettings.ValueDefault;
                    break;

                case EnumQuestionType.SummaryCompletionDropDown:
                    var summaryCompletionDropDown = HandleQuestion(config.Deserialize<MatchingTaskQuestion>(), isCreated);
                    result = isDisableAnswers ? ClearAnswers(summaryCompletionDropDown) : summaryCompletionDropDown;
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect(summaryCompletionDropDown) : ValueSettings.ValueDefault;
                    break;

                case EnumQuestionType.MultichoiceV1:
                    var multichoiceV1 = config.Deserialize<MultipleChoiceQuestionV1>();
                    result = isDisableAnswers ? ClearAnswers(multichoiceV1) : multichoiceV1;
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect(multichoiceV1) : ValueSettings.ValueDefault;
                    break;

                case EnumQuestionType.CheckListV1:
                    var checkList = HandleQuestion(config.Deserialize<CheckListQuestionV1>(), isCreated);
                    result = isDisableAnswers ? ClearAnswers(checkList) : checkList;
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect(checkList) : ValueSettings.ValueDefault;
                    break;

                case EnumQuestionType.SummaryCompletionGapFill:
                    var summaryCompletionGapFill = HandleQuestion(config.Deserialize<CheckListQuestionV1>(), isCreated);
                    result = isDisableAnswers ? ClearAnswers(summaryCompletionGapFill) : summaryCompletionGapFill;
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect(summaryCompletionGapFill) : ValueSettings.ValueDefault;
                    break;

                case EnumQuestionType.CompletionDiagrams:
                case EnumQuestionType.FlowChartCompletion:
                    var completionDiagrams = HandleQuestion(config.Deserialize<CheckListQuestionV1>(), isCreated);
                    result = isDisableAnswers ? ClearAnswers(completionDiagrams) : completionDiagrams;
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect(completionDiagrams) : ValueSettings.ValueDefault;
                    break;

                case EnumQuestionType.TableCompletion:
                    var tableCompletion = config.Deserialize<TableCompletionQuestion>();
                    tableCompletion = HandleQuestion(tableCompletion, isCreated, tableCompletion?.Rows);
                    result = isDisableAnswers ? ClearAnswers(tableCompletion) : tableCompletion;
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect(tableCompletion) : ValueSettings.ValueDefault;
                    break;

                default:
                    throw new ArgumentException("Invalid question type");
            }
            return (result, totalCorrect);
        }

        public static bool ValidateQuestionExamPractice(object? config, object? configOld, EnumQuestionType questionType)
        {
            switch (questionType)
            {
                case EnumQuestionType.Multichoice:
                case EnumQuestionType.Checklist:
                    var multichoice = config.Deserialize<MultipleChoiceQuestion>();
                    var multichoiceOld = configOld.Deserialize<MultipleChoiceQuestion>();

                    return IsMultipleChoiceQuestionChanged(multichoice, multichoiceOld);
            }

            return false;
        }

        private static bool IsMultipleChoiceQuestionChanged(MultipleChoiceQuestion? current, MultipleChoiceQuestion? old)
        {
            if (current == null || old == null)
            {
                return true;
            }
            if (!string.Equals(current.Name, old.Name, StringComparison.Ordinal))
            {
                return true;
            }
            if (current.Contents.Count != old.Contents.Count)
            {
                return true;
            }
            for (int i = 0; i < current.Contents.Count; i++)
            {
                var newItem = current.Contents[i];
                var oldItem = old.Contents[i];

                if (newItem.Id != oldItem.Id ||
                    !string.Equals(newItem.FilePath, oldItem.FilePath, StringComparison.Ordinal) ||
                    !string.Equals(newItem.Content, oldItem.Content, StringComparison.Ordinal) ||
                    newItem.IsCorrect != oldItem.IsCorrect)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool ValidateQuestionExamPractice(object? config, EnumQuestionType questionType)
        {
            var isError = false;
            switch (questionType)
            {
                case EnumQuestionType.Multichoice:
                case EnumQuestionType.Checklist:

                    var multichoice = config.Deserialize<MultipleChoiceQuestion>();
                    isError = ValidateMultichoice(multichoice);
                    break;

                default:
                    break;
            }
            return isError;
        }

        public static bool ValidateQuestion(object? config, EnumQuestionType questionType)
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
            if (data == null || !data.Answers.Any())
            {
                return isError;
            }
            if (HasInvalidKeysOrContent(data.Answers))
            {
                return isError;
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

        private static bool ValidateMultichoice(MultipleChoiceQuestion? data)
        {
            var isError = true;
            if (data == null || !data.Contents.Any())
            {
                return isError;
            }

            foreach (var item in data.Contents)
            {
                if (string.IsNullOrEmpty(item.Content))
                {
                    return isError;
                }
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

        private static dynamic? HandleQuestion(dynamic? data, bool isCreated = false, dynamic? dataList = null)
        {
            dataList ??= data;
            if (dataList == null)
            {
                return dataList;
            }
            if (dataList is IList list)
            {
                if (data?.GetType().GetProperty(nameof(data.Answers)) != null && isCreated)
                {
                    data?.Answers.Clear();
                }
                var listData = new List<dynamic>();
                foreach (dynamic item in list.OfType<dynamic>())
                {
                    listData.Add(HandleQuestion(data, isCreated, item));
                }
                return data;
            }

            var content = dataList.Content;
            if (string.IsNullOrEmpty(content))
            {
                return dataList;
            }
            if (data?.GetType().GetProperty(nameof(data.Answers)) != null && dataList.GetType().Name == data?.GetType().Name && isCreated)
            {
                data?.Answers.Clear();
            }
            MatchCollection matches = Regex.Matches(content, @"\{(.*?)\}");
            if (!matches.Any())
            {
                return dataList;
            }
            Dictionary<Guid, string?> replacements = new Dictionary<Guid, string?>();
            var propertyInfo = data?.GetType().GetProperty(nameof(data.Answers));
            foreach (Match match in matches)
            {
                string value = match.Groups[1].Value;
                if (Guid.TryParse(value, out _))
                {
                    continue;
                }
                var id = Guid.NewGuid();
                dynamic config;
                if (GetTypeData(propertyInfo).Name == nameof(ConfigAnswerV1))
                {
                    config = new ConfigAnswerV1 { Id = id, Key = value };
                }
                else
                {
                    config = new ConfigQuestionV1 { Id = id, Key = value };
                }
                if (data?.GetType().GetProperty(nameof(data.Rows)) != null)
                {
                    config.RowId = dataList.Id;
                }
                if (data?.GetType().GetProperty(nameof(data.Answers)) != null)
                {
                    data?.Answers.Add(config);
                }
                replacements[id] = value;
            }

            dataList.Content = ReplacePlaceholders(content, replacements);
            return dataList;
        }

        private static Type? GetTypeData(dynamic propertyInfo)
        {
            Type propertyType = propertyInfo.PropertyType;
            if (propertyType.IsGenericType && typeof(IEnumerable<>).MakeGenericType(propertyType.GetGenericArguments()).IsAssignableFrom(propertyType))
            {
                return propertyType.GetGenericArguments().FirstOrDefault();
            }
            return propertyType;
        }

        private static string? ReplacePlaceholders(string? content, Dictionary<Guid, string> replacements)
        {
            foreach (var pair in replacements)
            {
                if (!Guid.TryParse(pair.Value, out _))
                {
                    content = ReplaceFirst(content, "{" + pair.Value + "}", "{" + pair.Key + "}");
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
                    tableCompletion.Answers = new List<ConfigAnswerV1>();
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
                    dragAndDropList.Contents = GenerateRandomLoop(dragAndDropList.Contents);
                    return dragAndDropList;

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
                    return multipleChoice.Contents?.Count(x => x.IsCorrect.HasValue && x.IsCorrect.Value) ?? ValueSettings.ValueDefault;

                case MultipleOptionSentenceCompletionQuestion multipleOption:
                    return multipleOption.Contents?.Count ?? ValueSettings.ValueDefault;

                case MatchingTypeQuestion matchingType:
                    return matchingType.Link?.Count ?? ValueSettings.ValueDefault;

                case GapFillQuestion gapFill when !calculateByGap:
                    return gapFill.Contents?.Count ?? ValueSettings.ValueDefault;

                case GapFillQuestion gapFill when calculateByGap:
                    return gapFill.Contents?.Sum(x => x.Words?.Count ?? ValueSettings.ValueDefault) ?? ValueSettings.ValueDefault;

                case DragAndDropSentenceOrderQuestion dragAndDrop:
                    return dragAndDrop.Contents?.Count ?? ValueSettings.ValueDefault;

                // V1
                case MultipleChoiceQuestionV1 multipleChoiceV1:
                    return multipleChoiceV1.Answers?
                        .Where(x => x.Answers != null && x.Answers.Any())
                        .SelectMany(x => x.Answers!)
                        .Count(x => x.IsCorrect.HasValue && x.IsCorrect.Value) ?? ValueSettings.ValueDefault;

                case CheckListQuestionV1 checkList:
                    if (checkList.Answers != null)
                    {
                        return checkList.Answers.Any(x => x.IsCorrect.HasValue)
                            ? checkList.Answers.Count(x => x.IsCorrect.HasValue && x.IsCorrect.Value)
                            : checkList.Answers.Count;
                    }
                    return ValueSettings.ValueDefault;

                case MatchingTaskQuestion matchingTask:
                    return matchingTask.Answers?.Count ?? ValueSettings.ValueDefault;

                case TableCompletionQuestion tableCompletion:
                    return tableCompletion.Answers?.Count ?? ValueSettings.ValueDefault;

                case FlowChartCompletionQuestion flowChart:
                    return flowChart.Answers?.Count ?? ValueSettings.ValueDefault;

                default:
                    return 1;
            }
        }

        private static IList<T>? GenerateRandomLoop<T>(IList<T>? datas, IList<SubQuestionConfig>? shuffleConfigs = null)
        {
            if (datas == null)
            {
                return datas;
            }
            if (shuffleConfigs == null || !shuffleConfigs.Any())
            {
                var rand = new Random();
                return datas.OrderBy(_ => rand.Next()).ToList();
            }
            else
            {
                return datas.OrderBy(x => shuffleConfigs.FirstOrDefault(y => y.Id == x.GetPropValue("Id")?.ToString())?.Index ?? default).ThenBy(x => x.GetPropValue("Id")).ToList();
            }
        }

        public static (object?, string?) QuestionShuffleConverterObject(object? config, EnumQuestionType type, IList<SubQuestionConfig>? shuffleConfigs = null)
        {
            object? result = config;
            string questionShuffleStr = shuffleConfigs != null ? shuffleConfigs.Serialize() : string.Empty;
            switch (type)
            {
                case EnumQuestionType.MatchingType1:
                    var matchingTypeQuestion = config.Deserialize<MatchingTypeQuestion>();
                    if (matchingTypeQuestion != null)
                    {
                        matchingTypeQuestion.To = GenerateRandomLoop(matchingTypeQuestion.To, shuffleConfigs);
                        if (shuffleConfigs == null)
                        {
                            questionShuffleStr = matchingTypeQuestion.To?.Select((x, index) => new SubQuestionConfig
                            {
                                Id = $"{x.Id}",
                                Index = index
                            }).Serialize() ?? string.Empty;
                        }
                    }
                    result = matchingTypeQuestion;
                    break;
            }
            return (result, questionShuffleStr);
        }
    }
}
