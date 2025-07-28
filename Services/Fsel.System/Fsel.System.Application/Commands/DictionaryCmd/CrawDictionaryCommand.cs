// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.DictionaryCmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.IRepositories;
    using global::System.Globalization;
    using global::System.Text.Json;
    using global::System.Text.Json.Serialization;
    using global::System.Text.RegularExpressions;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class CrawDictionaryCommand : IRequest<MethodResult<bool>>
    {
        public string? Word { get; set; }
    }

    public class CrawDictionaryCommandHandler : IRequestHandler<CrawDictionaryCommand, MethodResult<bool>>
    {
        private const string BaseUrl = "https://api.tracau.vn/WBBcwnwQpV89/s/{0}/en";
        private readonly IDictionaryRepository _dictionaryRepository;
        private readonly IUnknownWordRepository _unknownWordRepository;
        private readonly HttpClient _httpClient;

        public CrawDictionaryCommandHandler(IDictionaryRepository dictionaryRepository, HttpClient httpClient, IUnknownWordRepository unknownWordRepository)
        {
            _dictionaryRepository = dictionaryRepository;
            _httpClient = httpClient;
            _unknownWordRepository = unknownWordRepository;
        }

        public async Task<MethodResult<bool>> Handle(CrawDictionaryCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            if (string.IsNullOrWhiteSpace(request.Word))
            {
                methodResult.AddError(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.Word));
                return methodResult;
            }

            var url = string.Format(CultureInfo.CurrentCulture, BaseUrl, Uri.EscapeDataString(request.Word));

            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync(cancellationToken);
            var data = JsonSerializer.Deserialize<DictionaryResponse>(jsonString);

            if (data == null)
            {
                return methodResult;
            }

            var formatJsonResult = ProcessData(data, request.Word);

            // Lưu lại những từ không tìm thấy trong từ điển
            if (!formatJsonResult.IsOK || formatJsonResult.Result == null)
            {
                var unKnownWord = await _unknownWordRepository.Queryable
                    .Where(w => w.Word.ToLower().Trim() == request.Word.ToLower().Trim())
                    .FirstOrDefaultAsync(cancellationToken);

                await _unknownWordRepository.ExecuteTransactionAsync(async () =>
                {
                    if (unKnownWord == null)
                    {
                        _unknownWordRepository.Add(new UnknownWord()
                        {
                            Word = request.Word,
                            Source = BaseUrl,
                        });
                    }
                    else
                    {
                        unKnownWord.NumberOfSearch += 1;
                        unKnownWord.Source = BaseUrl;
                        _unknownWordRepository.Update(unKnownWord);
                    }
                    await _unknownWordRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

                    methodResult.Result = true;
                    return methodResult;
                });
            }

            var dictionaries = HandleDataToInsert(formatJsonResult.Result);

            await _dictionaryRepository.ExecuteTransactionAsync(async () =>
            {
                await _dictionaryRepository.AddList(dictionaries);
                await _dictionaryRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

                methodResult.Result = true;
                return methodResult;
            });

            return methodResult;
        }

        private static IList<Dictionary> HandleDataToInsert(DictionaryData? data)
        {
            var result = new List<Dictionary>();

            if (data == null)
            {
                return result;
            }

            foreach (var partOfSpeech in data.PartOfSpeech)
            {
                var dictionaries = partOfSpeech.Meanings.Select(meaning => new Dictionary
                {
                    Word = data.Word,
                    Phonetic = data.Phonetic,
                    PartOfSpeech = partOfSpeech.Name,
                    Meaning = meaning.Text,
                    Examples = meaning.Examples.Select(ex => new Domain.Models.CommandModels.Dictionaries.Example
                    {
                        ExampleEn = ex.English,
                        ExampleVn = ex.Vietnamese
                    }).ToList()
                }).ToList();

                result.AddRange(dictionaries);
            }

            return result;
        }

        private static MethodResult<DictionaryData> ProcessData(DictionaryResponse data, string word)
        {
            var methodResult = new MethodResult<DictionaryData>();

            var result = new DictionaryData
            {
                Word = word,
                Phonetic = "",
                PartOfSpeech = new List<PartOfSpeech>()
            };

            try
            {
                if (data.Tratu != null && data.Tratu.Count > 0)
                {
                    var dictContent = data.Tratu[0];
                    if (dictContent.Fields?.Fulltext != null)
                    {
                        string html = dictContent.Fields.Fulltext;
                        int beginInx = html.IndexOf("<article id=\"dict_ev\" data-tab-name=\"Anh - Việt\"", StringComparison.Ordinal);
                        int laseInx = html.IndexOf("</article>", beginInx, StringComparison.Ordinal);

                        html = html.Substring(beginInx, laseInx - beginInx);

                        // Extract phonetic
                        var phoneticMatch = Regex.Match(html, @"<tr id=""pa""><td id=""I_C""><font color=""#9e9e9e"">◘</font></td><td id=""C_C""(?: colspan=""([1-9])"")?><font color=""#9e9e9e"">([^<]+)</font></td></tr>");
                        if (phoneticMatch.Success)
                        {
                            result.Phonetic = phoneticMatch.Groups[2].Value.Trim().Replace("[", "", StringComparison.Ordinal).Replace("]", "", StringComparison.Ordinal);
                        }

                        // Process part of speech
                        var allPartOfSpeechs = html.Split(new[] { "<tr id=\"tl\">" }, StringSplitOptions.RemoveEmptyEntries);
                        if (allPartOfSpeechs.Length > 0)
                        {
                            allPartOfSpeechs = allPartOfSpeechs.Skip(1).ToArray();
                            foreach (var partOfSpeech in allPartOfSpeechs)
                            {
                                string partOfSpeechHtml = "<tr id=\"tl\">" + partOfSpeech;
                                int cutInx = partOfSpeechHtml.IndexOf("</tr>", StringComparison.Ordinal);
                                string termPartOfSpeech = partOfSpeechHtml.Substring(0, cutInx + 5);

                                // Clean up HTML tags
                                termPartOfSpeech = Regex.Replace(termPartOfSpeech, @"<font color=""#1a76bf"">|</font>", "");
                                termPartOfSpeech = Regex.Replace(termPartOfSpeech, @"<font color=""#1371BB"">|</font>", "");

                                var partOfSpeechRex = Regex.Match(termPartOfSpeech, @"<tr id=""tl""><td id=""I_C"">\*</td><td id=""C_C""(?: colspan=""([1-9])"")?><b>([^<]+)</b></td></tr>");
                                if (partOfSpeechRex.Success)
                                {
                                    var partOfSpeechItem = new PartOfSpeech
                                    {
                                        Name = partOfSpeechRex.Groups[2].Value.Length > 0
                                            ? char.ToUpper(partOfSpeechRex.Groups[2].Value[0], CultureInfo.CurrentCulture) + partOfSpeechRex.Groups[2].Value.Substring(1)
                                            : "",
                                        Meanings = new List<Meaning>()
                                    };

                                    var allMeanings = partOfSpeechHtml.Split(new[] { "<tr id=\"mn\">" }, StringSplitOptions.RemoveEmptyEntries);
                                    if (allMeanings.Length > 1)
                                    {
                                        allMeanings = allMeanings.Skip(1).ToArray();
                                        foreach (var meaning in allMeanings)
                                        {
                                            var meaningHtml = "<tr id=\"mn\">" + meaning;
                                            int cutMeaningInx = meaningHtml.IndexOf("</tr>", StringComparison.Ordinal);
                                            var termMeaning = meaningHtml.Substring(0, cutMeaningInx + 5);
                                            int begin = termMeaning.IndexOf("(<font color=\"#1371BB\">", StringComparison.Ordinal);
                                            if (begin != -1)
                                            {
                                                int last = termMeaning.IndexOf("</font>)", StringComparison.Ordinal);
                                                termMeaning = $"{termMeaning.Substring(0, begin)} {termMeaning.Substring(last + 8)}";
                                            }

                                            var meaningRex = Regex.Match(termMeaning, @"<td id=""C_C""(?: colspan=""([1-9])"")?>([^<>]*)</td>");
                                            if (meaningRex.Success)
                                            {
                                                var meanObj = new Meaning
                                                {
                                                    Text = meaningRex.Groups[2].Value.Trim(),
                                                    Examples = new List<Example>()
                                                };

                                                var allExamples = meaningHtml.Split(new[] { "<tr id=\"mh\">" }, StringSplitOptions.RemoveEmptyEntries);
                                                if (allExamples.Length > 1)
                                                {
                                                    allExamples = allExamples.Skip(1).ToArray();
                                                    foreach (var example in allExamples)
                                                    {
                                                        string exampleHtml = "<tr id=\"mh\">" + example;
                                                        int cutExampleInx = exampleHtml.IndexOf("</tr>", StringComparison.Ordinal);
                                                        string tempExample = exampleHtml.Substring(0, cutExampleInx + 5);

                                                        tempExample = Regex.Replace(tempExample, @"<font color=""#1371BB"">|</font>", "");
                                                        tempExample = Regex.Replace(tempExample, @"<font color=""#7E7E7E"">|</font>", "");

                                                        var exampleEnRex = Regex.Match(tempExample, @"<td id=""C_C""(?: colspan=""([1-9])"")?>([^<]+)</td>");
                                                        var exampleVnRex = Regex.Match(exampleHtml, @"<td id=""C_C""(?: colspan=""([1-9])"")?><font color=""#7E7E7E"">([^<]+)</font></td>");

                                                        var exampleObj = new Example();
                                                        if (exampleEnRex.Success)
                                                        {
                                                            exampleObj.English = exampleEnRex.Groups[2].Value.Trim();
                                                        }
                                                        if (exampleVnRex.Success)
                                                        {
                                                            exampleObj.Vietnamese = exampleVnRex.Groups[2].Value.Trim();
                                                        }

                                                        if (!string.IsNullOrWhiteSpace(exampleObj.English) || !string.IsNullOrWhiteSpace(exampleObj.Vietnamese))
                                                        {
                                                            meanObj.Examples.Add(exampleObj);
                                                        }
                                                    }
                                                }

                                                partOfSpeechItem.Meanings.Add(meanObj);
                                            }
                                        }
                                    }

                                    result.PartOfSpeech.Add(partOfSpeechItem);
                                }
                            }
                        }
                    }
                    else
                    {
                        methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), word);
                        return methodResult;
                    }
                }
                else
                {
                    methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), word);
                    return methodResult;
                }
            }
            catch
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), word);
                return methodResult;
            }

            methodResult.Result = result;
            return methodResult;
        }
    }

    public class DictionaryResponse
    {
        [JsonPropertyName("tratu")]
        public List<TratuItem>? Tratu { get; set; }
    }

    public class TratuItem
    {
        [JsonPropertyName("fields")]
        public TratuFields? Fields { get; set; }
    }

    public class TratuFields
    {
        [JsonPropertyName("fulltext")]
        public string? Fulltext { get; set; }
    }

    public class DictionaryData
    {
        public string? Word { get; set; }
        public string? Phonetic { get; set; }
        public List<PartOfSpeech> PartOfSpeech { get; set; } = new List<PartOfSpeech>();
    }

    public class PartOfSpeech
    {
        public string? Name { get; set; }
        public List<Meaning> Meanings { get; set; } = new List<Meaning>();
    }

    public class Meaning
    {
        public string? Text { get; set; }
        public List<Example> Examples { get; set; } = new List<Example>();
    }

    public class Example
    {
        public string? English { get; set; }
        public string? Vietnamese { get; set; }
    }
}
