// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Common
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Fsel.Common.Helpers;
    using Fsel.Shared.Helpers;

    public class LinQAnswerHelper
    {
        public bool IsNullOrEmptyData(object? data, string? nameProperty = default, bool isLoop = true)
        {
            if (data is IList list)
            {
                var objects = list.Cast<object>().ToList();
                if (objects != null && objects.Any())
                {
                    if (isLoop)
                    {
                        return objects.Any(x => string.IsNullOrEmpty(nameProperty) ? IsNullOrEmptyData(x) : IsNullOrEmptyData(x.GetPropValue(nameProperty)));
                    }
                    else
                    {
                        return objects.Any(x => x.GetPropValue<bool>(nameProperty));
                    }
                }
                return true;
            }
            return string.IsNullOrEmpty(data?.ToString());
        }

        public bool IsAnswerHaveData(object? data, string? nameProperty = default)
        {
            if (data is IList list)
            {
                var objects = list.Cast<object>().ToList();
                if (objects != null && objects.Any())
                {
                    return objects.Any(x => !string.IsNullOrEmpty(nameProperty) ? IsAnswerHaveData(x.GetPropValue(nameProperty)) : IsAnswerHaveData(x));
                }
                return false;
            }
            return !string.IsNullOrEmpty(data?.ToString());
        }

        public bool IsDuplicateAnswerId(object? data, string? nameProperty = default)
        {
            if (data is IList list)
            {
                var objects = list.Cast<object>().ToList();
                if (objects != null && objects.Any())
                {
                    return objects.GroupBy(x => x.GetPropValue(nameProperty)).Any(x => x.Count() > 1);
                }
                return false;
            }
            return !string.IsNullOrEmpty(data?.ToString());
        }

        public bool CheckAnswerCount(object? answer, object? question)
        {
            if (answer is IList listAnswer && question is IList listQuestion)
            {
                return listAnswer.Count == listQuestion.Count;
            }
            return true;
        }

        public bool? CheckAnswer(IList<string>? words, string? word, int index = default)
        {
            if (words != null && words.Any() && !string.IsNullOrEmpty(word))
            {
                if (words[index].IndexOf('|', StringComparison.Ordinal) != -1)
                {
                    string[] questionWords = words[index].Split('|');
                    foreach (var item in questionWords)
                    {
                        if (word.ReplaceWord() == item.ReplaceWord())
                        {
                            return true;
                        }
                    }
                }
                else if (words[index].ReplaceWord() == word.ReplaceWord())
                {
                    return true;
                }
            }
            return false;
        }

        public bool CheckAnswer(string? content, string? word)
        {
            if (string.IsNullOrEmpty(content))
            {
                return false;
            }
            if (content.IndexOf('|', StringComparison.Ordinal) != -1)
            {
                string[] questionWords = content.Split('|');
                foreach (var item in questionWords)
                {
                    if (word.ReplaceWord() == item.ReplaceWord())
                    {
                        return true;
                    }
                }
            }
            else if (content.ReplaceWord() == word.ReplaceWord())
            {
                return true;
            }
            return false;
        }

        public bool IsShortAnswer(string? question, string? answer)
        {
            string q = " " + question.ReplaceWord() + " ";
            string a = " " + answer.ReplaceWord() + " ";
            return a.Contains(q, StringComparison.OrdinalIgnoreCase);
        }
    }
}
