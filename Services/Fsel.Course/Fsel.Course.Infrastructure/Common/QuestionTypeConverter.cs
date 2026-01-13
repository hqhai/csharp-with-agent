// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Text.RegularExpressions;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.QuestionTypeConfigs;
    using Fsel.Course.Domain.Entities.QuestionTypeConfigs.Questions;
    using Fsel.Course.Domain.Entities.QuestionTypeConfigs.Questions.V1i1;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Microsoft.EntityFrameworkCore;

    public class QuestionTypeConverter
    {
        private readonly IKeyboardTextRepository _keyboardTextRepository;

        public QuestionTypeConverter(IKeyboardTextRepository keyboardTextRepository)
        {
            _keyboardTextRepository = keyboardTextRepository;
        }

        public (object?, int) QuestionTypeConverterObject(object? config, EnumQuestionType type, bool isShowCorrectTotal = false, bool isDisableAnswers = false, bool isCreated = false)
        {
            int totalCorrect = default;
            object? result = null;
            switch (type)
            {
                case EnumQuestionType.Multichoice:
                case EnumQuestionType.Dropdown:
                    var multichoice = config.Deserialize<MultipleChoiceQuestion>();
                    if (multichoice != null)
                    {
                        result = isDisableAnswers ? ClearAnswers(multichoice) : multichoice;
                        totalCorrect = isShowCorrectTotal ? GetTotalCorrect(multichoice) : ValueSettings.ValueDefault;
                    }
                    break;

                case EnumQuestionType.Checklist:
                    var checklist = config.Deserialize<MultipleChoiceQuestion>();
                    if (checklist != null)
                    {
                        result = isDisableAnswers ? ClearAnswers(checklist) : checklist;
                        totalCorrect = isShowCorrectTotal ? GetTotalCorrect(checklist) : ValueSettings.ValueDefault;
                    }
                    break;

                case EnumQuestionType.Listing:
                    result = config.Deserialize<ListingQuestion>();
                    if (result != null)
                    {
                        totalCorrect = isShowCorrectTotal ? GetTotalCorrect(result) : ValueSettings.ValueDefault;
                    }
                    break;

                case EnumQuestionType.MatchingType1:
                case EnumQuestionType.MatchingType2:
                case EnumQuestionType.DragAndDropPicture:
                    var matchingTypeQuestion = config.Deserialize<MatchingTypeQuestion>();
                    if (matchingTypeQuestion != null)
                    {
                        result = isDisableAnswers ? ClearAnswers(matchingTypeQuestion) : matchingTypeQuestion;
                        totalCorrect = isShowCorrectTotal ? GetTotalCorrect(matchingTypeQuestion) : ValueSettings.ValueDefault;
                    }
                    break;

                case EnumQuestionType.ShortAnswerWordBase:
                    var shortAnswerQuestionWordBaseQuestion = config.Deserialize<ShortAnswerQuestionWordBaseQuestion>();
                    if (shortAnswerQuestionWordBaseQuestion != null)
                    {
                        result = isDisableAnswers ? ClearAnswers(shortAnswerQuestionWordBaseQuestion) : shortAnswerQuestionWordBaseQuestion;
                        totalCorrect = isShowCorrectTotal ? GetTotalCorrect(result) : ValueSettings.ValueDefault;
                    }
                    break;

                case EnumQuestionType.ShortAnswerWordCount:
                    result = config.Deserialize<ShortAnswerQuestionWordCountBaseQuestion>();
                    if (result != null)
                    {
                        totalCorrect = isShowCorrectTotal ? GetTotalCorrect(result) : ValueSettings.ValueDefault;
                    }
                    break;

                case EnumQuestionType.GapFillScoreByQuestion:
                    var gapFillQuestion = config.Deserialize<GapFillQuestion>();
                    if (gapFillQuestion != null)
                    {
                        result = isDisableAnswers ? ClearAnswers(gapFillQuestion) : gapFillQuestion;
                        totalCorrect = isShowCorrectTotal ? GetTotalCorrect(gapFillQuestion) : ValueSettings.ValueDefault;
                    }
                    break;

                case EnumQuestionType.GapFillWordBankScoreByQuestion:
                    var gapFillWordBankScoreQuestion = config.Deserialize<GapFillQuestion>();
                    if (gapFillWordBankScoreQuestion != null)
                    {
                        result = isDisableAnswers ? ClearAnswers(gapFillWordBankScoreQuestion) : gapFillWordBankScoreQuestion;
                        totalCorrect = isShowCorrectTotal ? GetTotalCorrect(gapFillWordBankScoreQuestion) : ValueSettings.ValueDefault;
                    }
                    break;

                case EnumQuestionType.GapFillWordBankScoreByGap:
                    var gapFillWordBankScoreByGap = config.Deserialize<GapFillQuestion>();
                    if (gapFillWordBankScoreByGap != null)
                    {
                        result = isDisableAnswers ? ClearAnswers(gapFillWordBankScoreByGap) : gapFillWordBankScoreByGap;
                        totalCorrect = isShowCorrectTotal ? GetTotalCorrect(gapFillWordBankScoreByGap, true) : ValueSettings.ValueDefault;
                    }
                    break;

                case EnumQuestionType.GapFillScoreByGap:
                    var gapFillQuestionByGap = config.Deserialize<GapFillQuestion>();
                    if (gapFillQuestionByGap != null)
                    {
                        result = isDisableAnswers ? ClearAnswers(gapFillQuestionByGap) : gapFillQuestionByGap;
                        totalCorrect = isShowCorrectTotal ? GetTotalCorrect(gapFillQuestionByGap, true) : ValueSettings.ValueDefault;
                    }
                    break;

                case EnumQuestionType.DragAndDropSentenceOrder:
                    var dragAndDropSentenceOrderQuestion = config.Deserialize<DragAndDropSentenceOrderQuestion>();
                    if (dragAndDropSentenceOrderQuestion != null)
                    {
                        result = isDisableAnswers ? ClearAnswers(dragAndDropSentenceOrderQuestion) : dragAndDropSentenceOrderQuestion;
                        totalCorrect = isShowCorrectTotal ? GetTotalCorrect(dragAndDropSentenceOrderQuestion) : ValueSettings.ValueDefault;
                    }
                    break;

                case EnumQuestionType.DragAndDropListSentenceOrder:
                    var dragAndDropList = config.Deserialize<DragAndDropListSentenceOrderQuestion>();
                    if (dragAndDropList != null)
                    {
                        dragAndDropList.Contents = dragAndDropList.Contents?.Select((x, index) => { x.Id = ++index; return x; }).ToList();
                        result = isDisableAnswers ? ClearAnswers(dragAndDropList) : dragAndDropList;
                        totalCorrect = isShowCorrectTotal ? GetTotalCorrect(dragAndDropList) : ValueSettings.ValueDefault;
                    }
                    break;

                case EnumQuestionType.MultipleOptionSentenceCompletion:
                    var multipleOption = config.Deserialize<MultipleOptionSentenceCompletionQuestion>();
                    if (multipleOption != null)
                    {
                        result = isDisableAnswers ? ClearAnswers(multipleOption) : multipleOption;
                        totalCorrect = isShowCorrectTotal ? GetTotalCorrect(multipleOption) : ValueSettings.ValueDefault;
                    }
                    break;

                case EnumQuestionType.ExercisePreparation:
                    result = config.Deserialize<ExercisePreparationQuestion>();
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect(result) : ValueSettings.ValueDefault;
                    break;

                // Dạng câu hỏi mới
                case EnumQuestionType.MatchingParagraphInfo:
                    var matchingParagraphInfo = HandleQuestion(config.Deserialize<MatchingTaskQuestion>(), isCreated);
                    if (matchingParagraphInfo != null)
                    {
                        result = isDisableAnswers ? ClearAnswers(matchingParagraphInfo) : matchingParagraphInfo;
                        totalCorrect = isShowCorrectTotal ? GetTotalCorrect(matchingParagraphInfo) : ValueSettings.ValueDefault;
                    }
                    break;

                case EnumQuestionType.MatchingHeading:
                    var matchingHeading = HandleQuestion(config.Deserialize<MatchingTaskQuestion>(), isCreated);
                    if (matchingHeading != null)
                    {
                        result = isDisableAnswers ? ClearAnswers(matchingHeading) : matchingHeading;
                        totalCorrect = isShowCorrectTotal ? GetTotalCorrect(matchingHeading) : ValueSettings.ValueDefault;
                    }
                    break;

                case EnumQuestionType.YesNoNotGivenDropDown:
                    var yesNoNotGivenDropDown = HandleQuestion(config.Deserialize<MatchingTaskQuestion>(), isCreated);
                    if (yesNoNotGivenDropDown != null)
                    {
                        result = isDisableAnswers ? ClearAnswers(yesNoNotGivenDropDown) : yesNoNotGivenDropDown;
                        totalCorrect = isShowCorrectTotal ? GetTotalCorrect(yesNoNotGivenDropDown) : ValueSettings.ValueDefault;
                    }
                    break;

                case EnumQuestionType.TrueFalseNotGivenDropDown:
                    var trueFalseNotGivenDropDown = HandleQuestion(config.Deserialize<MatchingTaskQuestion>(), isCreated);
                    if (trueFalseNotGivenDropDown != null)
                    {
                        result = isDisableAnswers ? ClearAnswers(trueFalseNotGivenDropDown) : trueFalseNotGivenDropDown;
                        totalCorrect = isShowCorrectTotal ? GetTotalCorrect(trueFalseNotGivenDropDown) : ValueSettings.ValueDefault;
                    }
                    break;

                case EnumQuestionType.MapLabelingDropDown:
                    var mapLabelingDropDown = HandleQuestion(config.Deserialize<MatchingTaskQuestion>(), isCreated);
                    if (mapLabelingDropDown != null)
                    {
                        result = isDisableAnswers ? ClearAnswers(mapLabelingDropDown) : mapLabelingDropDown;
                        totalCorrect = isShowCorrectTotal ? GetTotalCorrect(mapLabelingDropDown) : ValueSettings.ValueDefault;
                    }
                    break;

                case EnumQuestionType.SummaryCompletionDropDown:
                    var summaryCompletionDropDown = HandleQuestion(config.Deserialize<MatchingTaskQuestion>(), isCreated);
                    if (summaryCompletionDropDown != null)
                    {
                        result = isDisableAnswers ? ClearAnswers(summaryCompletionDropDown) : summaryCompletionDropDown;
                        totalCorrect = isShowCorrectTotal ? GetTotalCorrect(summaryCompletionDropDown) : ValueSettings.ValueDefault;
                    }
                    break;

                case EnumQuestionType.MultichoiceV1:
                    var multichoiceV1 = config.Deserialize<MultipleChoiceQuestionV1>();
                    if (multichoiceV1 != null)
                    {
                        result = isDisableAnswers ? ClearAnswers(multichoiceV1) : multichoiceV1;
                        totalCorrect = isShowCorrectTotal ? GetTotalCorrect(multichoiceV1) : ValueSettings.ValueDefault;
                    }
                    break;

                case EnumQuestionType.CheckListV1:
                    var checkList = HandleQuestion(config.Deserialize<CheckListQuestionV1>(), isCreated);
                    if (checkList != null)
                    {
                        result = isDisableAnswers ? ClearAnswers(checkList) : checkList;
                        totalCorrect = isShowCorrectTotal ? GetTotalCorrect(checkList) : ValueSettings.ValueDefault;
                    }
                    break;

                case EnumQuestionType.SummaryCompletionGapFill:
                    var summaryCompletionGapFill = HandleQuestion(config.Deserialize<CheckListQuestionV1>(), isCreated);
                    if (summaryCompletionGapFill != null)
                    {
                        result = isDisableAnswers ? ClearAnswers(summaryCompletionGapFill) : summaryCompletionGapFill;
                        totalCorrect = isShowCorrectTotal ? GetTotalCorrect(summaryCompletionGapFill) : ValueSettings.ValueDefault;
                    }

                    break;

                case EnumQuestionType.CompletionDiagrams:
                case EnumQuestionType.FlowChartCompletion:
                    var completionDiagrams = HandleQuestion(config.Deserialize<CheckListQuestionV1>(), isCreated);
                    if (completionDiagrams != null)
                    {
                        result = isDisableAnswers ? ClearAnswers(completionDiagrams) : completionDiagrams;
                        totalCorrect = isShowCorrectTotal ? GetTotalCorrect(completionDiagrams) : ValueSettings.ValueDefault;
                    }
                    break;

                case EnumQuestionType.TableCompletion:
                    var tableCompletion = config.Deserialize<TableCompletionQuestion>();
                    if (tableCompletion != null)
                    {
                        tableCompletion = HandleQuestion(tableCompletion, isCreated, tableCompletion?.Rows);
                        result = isDisableAnswers ? ClearAnswers(tableCompletion) : tableCompletion;
                        totalCorrect = isShowCorrectTotal ? GetTotalCorrect(tableCompletion) : ValueSettings.ValueDefault;
                    }
                    break;

                case EnumQuestionType.Tracing:
                    var tracingQuestion = config.Deserialize<TracingQuestion>();
                    if (tracingQuestion != null)
                    {
                        tracingQuestion.KeyboardText = GetKeyboardText(tracingQuestion.KeyboardTextId);
                    }

                    result = tracingQuestion;
                    totalCorrect = ValueSettings.ValueDefaultTracingScore;
                    break;

                case EnumQuestionType.ColorMatchingType:
                    var colorMatchingTypeQuestion = config.Deserialize<ColorMatchingTypeQuestion>();
                    if (colorMatchingTypeQuestion != null)
                    {
                        result = colorMatchingTypeQuestion;
                        totalCorrect = GetTotalCorrect(colorMatchingTypeQuestion);
                    }
                    break;

                case EnumQuestionType.LongAnswer:
                    var longAnswerQuestion = config.Deserialize<LongAnswerQuestion>();
                    if (longAnswerQuestion != null)
                    {
                        result = longAnswerQuestion;
                        totalCorrect = GetTotalCorrect(longAnswerQuestion);
                    }
                    break;

                default:
                    throw new ArgumentException("Invalid question type");
            }
            return (result, totalCorrect);
        }

        public VoidMethodResult ValidateQuestionV1(object? config, EnumQuestionType questionType)
        {
            var method = new VoidMethodResult();
            switch (questionType)
            {
                case EnumQuestionType.ColorMatchingType:

                    var colorMatchingType = config.Deserialize<ColorMatchingTypeQuestion>();
                    if (colorMatchingType == null)
                    {
                        return method;
                    }
                    if (!colorMatchingType.IsValid())
                    {
                        method.AddErrorBadRequest(colorMatchingType.ErrorMessages);
                        return method;
                    }

                    if (!colorMatchingType.Contents.Any(x => x.IsCorrect == true))
                    {
                        method.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(ColorMatchingTypeQuestionOption.IsCorrect), colorMatchingType.Contents);
                        return method;
                    }
                    foreach (var item in colorMatchingType.Contents)
                    {
                        if (!item.IsValid())
                        {
                            method.AddErrorBadRequest(item.ErrorMessages);
                            return method;
                        }
                    }
                    break;

                default:
                    break;
            }
            return method;
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

                case TracingQuestion tracingQuestion:

                    break;

                case ColorMatchingTypeQuestion colorMatchingType:
                    // Clear correct answers for display
                    if (colorMatchingType.Contents != null)
                    {
                        foreach (var item in colorMatchingType.Contents)
                        {
                            item.IsCorrect = default;
                        }
                    }
                    break;
            }
            return data;
        }

        private static int GetTotalCorrect<T>(T? data, bool calculateByGap = false) where T : class
        {
            if (data == null)
            {
                return ValueSettings.ValueDefault;
            }
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

                case ColorMatchingTypeQuestion colorMatchingType:
                    return colorMatchingType.Contents?.Count(x => x.IsCorrect.HasValue && x.IsCorrect.Value) ?? ValueSettings.ValueDefault;

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

        public (object?, string?) QuestionShuffleConverterObject(object? config, EnumQuestionType type, bool isResultDone = false, IList<SubQuestionConfig>? shuffleConfigs = null)
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

                //case EnumQuestionType.GapFillWordBankScoreByGap:
                //    var gapFillQuestion = config.Deserialize<GapFillQuestion>();
                //    if (gapFillQuestion != null && !isResultDone)
                //    {
                //        gapFillQuestion.Contents = GenerateRandomLoop(gapFillQuestion.Contents, shuffleConfigs);
                //        if (shuffleConfigs == null)
                //        {
                //            questionShuffleStr = gapFillQuestion.Contents?.Select((x, index) => new SubQuestionConfig
                //            {
                //                Id = $"{x.Id}",
                //                Index = index
                //            }).Serialize() ?? string.Empty;
                //        }
                //    }
                //    result = gapFillQuestion;
                //    break;

                case EnumQuestionType.DragAndDropListSentenceOrder:
                    var dragAndDropList = config.Deserialize<DragAndDropListSentenceOrderQuestion>();
                    if (dragAndDropList != null && !isResultDone)
                    {
                        dragAndDropList.Contents = GenerateRandomLoop(dragAndDropList.Contents, shuffleConfigs);
                        if (shuffleConfigs == null)
                        {
                            questionShuffleStr = dragAndDropList.Contents?.Select((x, index) => new SubQuestionConfig
                            {
                                Id = $"{x.Id}",
                                Index = index
                            }).Serialize() ?? string.Empty;
                        }
                    }
                    result = dragAndDropList;
                    break;

                case EnumQuestionType.DragAndDropSentenceOrder:
                {
                    var drag = config.Deserialize<DragAndDropSentenceOrderQuestion>();
                    if (drag?.Contents == null || drag.Contents.Count == 0)
                    {
                        result = drag;
                        break;
                    }
                    if (isResultDone)
                    {
                        result = drag;
                        break;
                    }

                    // Map config theo ContentId
                    var map = shuffleConfigs?
                        .Where(x => !string.IsNullOrWhiteSpace(x.Id) && x.Indexes != null && x.Indexes.Count > 0)
                        .ToDictionary(x => x.Id!, x => x.Indexes!)
                        ?? new Dictionary<string, IList<int>>();

                    var generated = new List<SubQuestionConfig>();

                    foreach (var c in drag.Contents)
                    {
                        if (c?.Words == null || c.Words.Count <= 1)
                            continue;

                        var key = $"{c.Id}";
                        var n = c.Words.Count;

                        if (map.TryGetValue(key, out var idx) && IsValidPermutation(idx, n))
                        {
                            // ✅ Có config => sort lại đúng theo lần random đã lưu
                            c.Words = ApplyPermutation(c.Words, idx);
                        }
                        else
                        {
                            // ✅ Không có config => random + lưu lại
                            var perm = GeneratePermutation(n);   // perm là oldIndex theo thứ tự newIndex
                            c.Words = ApplyPermutation(c.Words, perm);

                            generated.Add(new SubQuestionConfig
                            {
                                Id = key,
                                Indexes = perm
                            });
                        }
                    }

                    // Lần đầu generate => lưu config
                    if (shuffleConfigs == null)
                    {
                        questionShuffleStr = generated.Serialize() ?? string.Empty;
                    }

                    result = drag;
                    break;
                }
            }
            return (result, questionShuffleStr);
        }

        private static IList<int> GeneratePermutation(int n)
        {
            var rng = new Random();
            var idx = Enumerable.Range(0, n).ToList();

            for (int i = n - 1; i > 0; i--)
            {
                var j = rng.Next(i + 1);
                (idx[i], idx[j]) = (idx[j], idx[i]);
            }

            return idx;
        }

        private static bool IsValidPermutation(IList<int> indexes, int n)
        {
            if (indexes.Count != n)
                return false;
            if (indexes.Any(i => i < 0 || i >= n))
                return false;
            return indexes.Distinct().Count() == n;
        }

        private static IList<string> ApplyPermutation(IList<string> words, IList<int> indexes)
        {
            var arr = words.ToArray();
            var res = new string[words.Count];

            for (int i = 0; i < indexes.Count; i++)
            {
                res[i] = arr[indexes[i]];
            }

            return res;
        }

        private KeyboardTextModel? GetKeyboardText(Guid keyboardTextId)
        {
            return _keyboardTextRepository.ReadOnlyDbContext.Set<KeyboardText>().Select(keyboardText => new KeyboardTextModel
            {
                Id = keyboardText.Id,
                Name = keyboardText.Name,
                Unicode = keyboardText.Unicode,
                FilePath = keyboardText.FilePath,
                VectorLibrary = keyboardText.VectorLibrary
            }).FirstOrDefault(x => x.Id == keyboardTextId);
        }
    }
}
