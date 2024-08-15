// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common
{
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
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect() : default;
                    break;

                case EnumQuestionType.Checklist:
                    var checklist = config.Deserialize<MultipleChoiceQuestion>();
                    result = isDisableAnswers ? ClearAnswers(checklist) : checklist;
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect(checklist) : default;
                    break;

                case EnumQuestionType.Listing:
                    result = config.Deserialize<ListingQuestion>();
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect() : default;
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
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect() : default;
                    break;

                case EnumQuestionType.ShortAnswerWordCount:
                    result = config.Deserialize<ShortAnswerQuestionWordCountBaseQuestion>();
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect() : default;
                    break;

                case EnumQuestionType.GapFillScoreByQuestion:
                    var gapFillQuestion = config.Deserialize<GapFillQuestion>();
                    result = isDisableAnswers ? ClearAnswers(gapFillQuestion) : gapFillQuestion;
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrectBySubQuestion(gapFillQuestion) : default;
                    break;

                case EnumQuestionType.GapFillWordBankScoreByQuestion:
                    var gapFillWordBankScoreQuestion = config.Deserialize<GapFillQuestion>();
                    result = isDisableAnswers ? ClearAnswers(gapFillWordBankScoreQuestion) : gapFillWordBankScoreQuestion;
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrectBySubQuestion(gapFillWordBankScoreQuestion) : default;
                    break;

                case EnumQuestionType.GapFillWordBankScoreByGap:
                    var gapFillWordBankScoreByGap = config.Deserialize<GapFillQuestion>();
                    result = isDisableAnswers ? ClearAnswers(gapFillWordBankScoreByGap) : gapFillWordBankScoreByGap;
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrectByGap(gapFillWordBankScoreByGap) : default;
                    break;

                case EnumQuestionType.GapFillScoreByGap:
                    var gapFillQuestionByGap = config.Deserialize<GapFillQuestion>();
                    result = isDisableAnswers ? ClearAnswers(gapFillQuestionByGap) : gapFillQuestionByGap;
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrectByGap(gapFillQuestionByGap) : default;
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
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect() : default;
                    break;

                case EnumQuestionType.MultipleOptionSentenceCompletion:
                    var multipleOption = config.Deserialize<MultipleOptionSentenceCompletionQuestion>();
                    result = isDisableAnswers ? ClearAnswers(multipleOption) : multipleOption;
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect(multipleOption) : default;
                    break;

                case EnumQuestionType.ExercisePreparation:
                    result = config.Deserialize<ExercisePreparationQuestion>();
                    totalCorrect = isShowCorrectTotal ? GetTotalCorrect() : default;
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
                    var flowChartCompletion = config.Deserialize<FlowChartCompletionQuestion>();
                    isError = ValidatFlowChartCompletion(flowChartCompletion);
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
                    if (!Guid.TryParse(id, out _))
                    {
                        var config = new AnswerTable
                        {
                            RowId = item.Id ?? Guid.NewGuid(),
                            Content = match.Groups[1].Value
                        };
                        if (data.AnswerTables.Any() && data.AnswerTables.Count >= replacements.Count)
                        {
                            data.AnswerTables.Insert(replacements.Count, config);
                        }
                        else
                        {
                            data.AnswerTables.Add(config);
                        }

                        replacements[id] = config.Id;
                    }
                    else
                    {
                        replacements[id] = new Guid(id);
                    }
                }

                foreach (var pair in replacements)
                {
                    if (!Guid.TryParse(pair.Key, out _))
                    {
                        item.Content = item.Content.Replace("{" + pair.Key + "}", "{" + pair.Value.ToString() + "}", StringComparison.CurrentCulture);
                    }
                }
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

                    foreach (Match match in matches)
                    {
                        string id = match.Groups[1].Value;
                        if (!Guid.TryParse(id, out _))
                        {
                            var config = new ConfigQuestionV1
                            {
                                Content = match.Groups[1].Value
                            };
                            if (data.Answers.Any() && data.Answers.Count >= replacements.Count)
                            {
                                data.Answers.Insert(replacements.Count, config);
                            }
                            else
                            {
                                data.Answers.Add(config);
                            }

                            replacements[id] = config.Id;
                        }
                        else
                        {
                            replacements[id] = new Guid(id);
                        }
                    }

                    foreach (var pair in replacements)
                    {
                        if (!Guid.TryParse(pair.Key, out _))
                        {
                            item2.Content = item2.Content.Replace("{" + pair.Key + "}", "{" + pair.Value.ToString() + "}", StringComparison.CurrentCulture);
                        }
                    }
                }
            }

            return data;
        }

        private static CheckListQuestionV1? HandleQuestion(CheckListQuestionV1? data)
        {
            if (data == null || string.IsNullOrEmpty(data.Content))
            {
                return data;
            }
            MatchCollection matches = Regex.Matches(data.Content, @"\{(.*?)\}");
            Dictionary<string, Guid?> replacements = new Dictionary<string, Guid?>();
            foreach (Match match in matches)
            {
                string id = match.Groups[1].Value;
                if (!Guid.TryParse(id, out _))
                {
                    var config = new ConfigQuestionV1
                    {
                        Content = match.Groups[1].Value
                    };

                    if (data.Answers.Any() && data.Answers.Count >= replacements.Count)
                    {
                        data.Answers.Insert(replacements.Count, config);
                    }
                    else
                    {
                        data.Answers.Add(config);
                    }

                    replacements[id] = config.Id;
                }
                else
                {
                    replacements[id] = new Guid(id);
                }
            }

            foreach (var pair in replacements)
            {
                if (!Guid.TryParse(pair.Key, out _))
                {
                    data.Content = data.Content.Replace("{" + pair.Key + "}", "{" + pair.Value.ToString() + "}", StringComparison.CurrentCulture);
                }
            }
            return data;
        }

        private static MatchingTaskQuestion? HandleQuestion(MatchingTaskQuestion? data)
        {
            if (data == null || string.IsNullOrEmpty(data.Content))
            {
                return data;
            }
            MatchCollection matches = Regex.Matches(data.Content, @"\{(.*?)\}");
            Dictionary<string, Guid?> replacements = new Dictionary<string, Guid?>();
            foreach (Match match in matches)
            {
                string id = match.Groups[1].Value;
                if (!Guid.TryParse(id, out _))
                {
                    var config = new ConfigQuestionV1
                    {
                        Key = match.Groups[1].Value
                    };
                    data.Answers.Insert(replacements.Count, config);
                    replacements[id] = config.Id;
                }
                else
                {
                    replacements[id] = new Guid(id);
                }
            }
            foreach (var pair in replacements)
            {
                if (!Guid.TryParse(pair.Key, out _))
                {
                    data.Content = data.Content.Replace("{" + pair.Key + "}", "{" + pair.Value.ToString() + "}", StringComparison.CurrentCulture);
                }
            }
            return data;
        }

        private static object? ClearAnswers(MatchingTaskQuestion? data)
        {
            if (data != null && data.Answers != null)
            {
                foreach (var item in data.Answers)
                {
                    item.Key = null;
                }
            }
            return data;
        }

        private static object? ClearAnswers(TableCompletionQuestion? data)
        {
            if (data != null)
            {
                data.AnswerTables = new List<AnswerTable>();
            }
            return data;
        }

        private static object? ClearAnswers(FlowChartCompletionQuestion? data)
        {
            if (data != null)
            {
                data.Answers = new List<ConfigAnswerV1>();
            }
            return data;
        }

        private static object? ClearAnswers(CheckListQuestionV1? data)
        {
            if (data != null && data.Answers != null)
            {
                foreach (var item in data.Answers)
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
            return data;
        }

        private static object? ClearAnswers(MultipleChoiceQuestionV1? data)
        {
            if (data != null && data.Answers != null)
            {
                foreach (var item in data.Answers.SelectMany(x => x.Answers))
                {
                    item.IsCorrect = null;
                }
            }
            return data;
        }

        private static object? ClearAnswers(MultipleOptionSentenceCompletionQuestion? data)
        {
            if (data != null && data.Contents != null)
            {
                foreach (var item in data.Contents)
                {
                    if (item != null)
                    {
                        item.Answers.ForEach(x =>
                        {
                            x.IsCorrect = null;
                        });
                    }
                }
            }
            return data;
        }

        private static object? ClearAnswers(ShortAnswerQuestionWordBaseQuestion? data)
        {
            if (data != null && data.Content != null && data.Content.Any())
            {
                data.Content.Clear();
            }
            return data;
        }

        private static object? ClearAnswers(MultipleChoiceQuestion? data)
        {
            if (data != null && data.Contents != null)
            {
                for (int i = data.Contents.Count - 1; i >= 0; i--)
                {
                    data.Contents[i].IsCorrect = default;
                }
            }
            return data;
        }

        private static object? ClearAnswers(DragAndDropSentenceOrderQuestion? data)
        {
            if (data != null && data.Contents != null)
            {
                foreach (var item in data.Contents)
                {
                    item.Words = GenerateRandomLoop(item.Words);
                }
            }
            return data;
        }

        private static DragAndDropListSentenceOrderQuestion? ClearAnswers(DragAndDropListSentenceOrderQuestion? data)
        {
            var contents = GenerateRandomLoop(data?.Contents);
            if (data != null)
            {
                data.Contents = contents;
            }
            return data;
        }

        private static object? ClearAnswers(MatchingTypeQuestion? data)
        {
            if (data != null && data.Link != null)
            {
                data.Link.Clear();
            }
            return data;
        }

        private static object? ClearAnswers(GapFillQuestion? data)
        {
            if (data != null && data.Contents != null)
            {
                foreach (var item in data.Contents)
                {
                    item.Words = GenerateRandomLoop(item.Words);
                }
            }
            return data;
        }

        private static int GetTotalCorrect(TableCompletionQuestion? data)
        {
            if (data != null && data.AnswerTables != null && data.AnswerTables.Any())
            {
                return data.AnswerTables.Count;
            }
            return default;
        }

        private static int GetTotalCorrect(FlowChartCompletionQuestion? data)
        {
            if (data != null && data.Answers != null && data.Answers.Any())
            {
                return data.Answers.Count;
            }
            return default;
        }

        private static int GetTotalCorrect(CheckListQuestionV1? data)
        {
            if (data != null && data.Answers != null && data.Answers.Any())
            {
                if (data.Answers.Any(x => x.IsCorrect.HasValue))
                {
                    return data.Answers.Count(x => x.IsCorrect.HasValue && x.IsCorrect.Value);
                }
                return data.Answers.Count;
            }
            return default;
        }

        private static int GetTotalCorrect(MultipleOptionSentenceCompletionQuestion? data)
        {
            if (data != null && data.Contents != null)
            {
                return data.Contents.Count;
            }
            return default;
        }

        private static int GetTotalCorrect(MultipleChoiceQuestion? data)
        {
            int number = 0;
            if (data != null && data.Contents != null)
            {
                foreach (var item in data.Contents)
                {
                    if (item.IsCorrect == true)
                    {
                        number++;
                    }
                }
                return data.Contents.Where(x => x.IsCorrect == true).Count();
            }
            return default;
        }

        private static int GetTotalCorrect()
        {
            return 1;
        }

        private static int GetTotalCorrect(MatchingTaskQuestion? data)
        {
            if (data != null && data.Answers != null)
            {
                return data.Answers.Count;
            }
            return default;
        }

        private static int GetTotalCorrect(MultipleChoiceQuestionV1? data)
        {
            if (data != null && data.Answers != null)
            {
                return data.Answers.Where(x => x.Answers != null && x.Answers.Any()).SelectMany(x => x.Answers!).Count(x => x.IsCorrect.HasValue && x.IsCorrect.Value);
            }
            return default;
        }

        private static int GetTotalCorrect(MatchingTypeQuestion? data)
        {
            if (data != null && data.Link != null)
            {
                return data.Link.Count;
            }
            return default;
        }

        private static int GetTotalCorrectBySubQuestion(GapFillQuestion? data)
        {
            if (data != null && data.Contents != null)
            {
                return data.Contents.Count;
            }
            return default;
        }

        private static int GetTotalCorrectByGap(GapFillQuestion? data)
        {
            if (data != null && data.Contents != null)
            {
                return data.Contents.Sum(x => x.Words?.Count ?? default);
            }
            return default;
        }

        private static int GetTotalCorrect(DragAndDropSentenceOrderQuestion? data)
        {
            if (data != null && data.Contents != null)
            {
                return data.Contents.Count;
            }
            return default;
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
