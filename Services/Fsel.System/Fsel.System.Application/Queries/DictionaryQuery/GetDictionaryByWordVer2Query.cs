// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.DictionaryQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.System.Application.Queues.Publisher;
    using Fsel.System.Application.Services.DictionaryServices;
    using Fsel.System.Application.Services.DictionaryServices.Models;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.IRepositories;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetDictionaryByWordVer2Query : IRequest<MethodResult<IList<DictionaryQueueItemModel>>>
    {
        public string? Word { get; set; }
    }

    public class GetDictionaryByWordVer2QueryHandler : IRequestHandler<GetDictionaryByWordVer2Query, MethodResult<IList<DictionaryQueueItemModel>>>
    {
        private readonly IDictionaryRepository _dictionaryRepository;
        private readonly IDictionaryService _dictionaryService;
        private readonly DictionaryPublisher _dictionaryPublisher;
        private readonly CrawDictionaryDataPublisher _crawDictionaryDataPublisher;

        public GetDictionaryByWordVer2QueryHandler(IDictionaryRepository dictionaryRepository, DictionaryPublisher dictionaryPublisher, IDictionaryService dictionaryService, CrawDictionaryDataPublisher crawDictionaryDataPublisher)
        {
            _dictionaryRepository = dictionaryRepository;
            _dictionaryPublisher = dictionaryPublisher;
            _dictionaryService = dictionaryService;
            _crawDictionaryDataPublisher = crawDictionaryDataPublisher;
        }

        public async Task<MethodResult<IList<DictionaryQueueItemModel>>> Handle(GetDictionaryByWordVer2Query request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<DictionaryQueueItemModel>>();

            var dictionaryOfWord = await _dictionaryRepository.Queryable
                .Where(x => x.Word.ToLower().Trim() == request.Word.ToLower().Trim())
                .ToListAsync(cancellationToken);

            var responseResult = await _dictionaryService.GetDictionaryByWordAsync(request.Word);

            var result = responseResult.Content?.FirstOrDefault();

            var phonetics = result?.Phonetics?.Where(p => !string.IsNullOrWhiteSpace(p.Text));

            var phonetic = phonetics?.FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.Audio)) ?? phonetics?.FirstOrDefault();

            var synonyms = result?.Meanings?.SelectMany(m => m.Synonyms ?? new List<string>()).ToList();
            var antonyms = result?.Meanings?.SelectMany(m => m.Antonyms ?? new List<string>()).ToList();

            var dictionaryOfWordModels = new List<DictionaryQueueItemModel>();

            // Lấy từ điển Anh - Việt
            if (dictionaryOfWord != null && dictionaryOfWord.Count > 0)
            {
                var dictionaryOfWordVnModel = GetVnDictionary(dictionaryOfWord, synonyms, antonyms, phonetic, dictionaryOfWord.FirstOrDefault()?.Phonetic);
                dictionaryOfWordModels.Add(dictionaryOfWordVnModel);
            }
            else
            {
                // Nếu không có từ điển Anh - Việt thì tự động craw lại từ đó
                await _crawDictionaryDataPublisher.Publish(request.Word, cancellationToken);
            }

            // Lấy từ điển Anh - Anh
            if (result != null)
            {
                var dictionaryOfWordEnModel = GetEnDictionary(result, synonyms, antonyms, phonetic);
                dictionaryOfWordModels.Add(dictionaryOfWordEnModel);
            }

            // Gửi dữ liệu realtime
            await _dictionaryPublisher.Publish(new DictionaryQueueModel
            {
                Dictionary = dictionaryOfWordModels
            }, cancellationToken);

            methodResult.Result = dictionaryOfWordModels;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private static DictionaryQueueItemModel GetEnDictionary(DictionaryModel dictionaryOfWord, IList<string>? synonyms, IList<string>? antonyms, Services.DictionaryServices.Models.PhoneticModel? phonetic)
        {
            var dictionaryOfWordModel = new DictionaryQueueItemModel()
            {
                Word = dictionaryOfWord.Word,
                Type = EnumDictionaryType.EnglishToEnglish.ToString(),
                Synonyms = synonyms,
                Antonyms = antonyms,
                Phonetic = new Shared.Models.ShareModels.PhoneticModel()
                {
                    Audio = phonetic?.Audio,
                    Text = phonetic?.Text
                },
                PartOfSpeechs = dictionaryOfWord.Meanings?.Select(x => new PartOfSpeechModel()
                {
                    PartOfSpeech = x.PartOfSpeech,
                    Definitions = x.Definitions?.Select(d => new Shared.Models.ShareModels.DefinitionsModel()
                    {
                        Definition = d.Definition,
                        Examples = string.IsNullOrWhiteSpace(d.Example) ? null : new List<Example>()
                        {
                            new Example{ ExampleEn = d.Example }
                        }
                    }).ToList()
                }).ToList()
            };

            return dictionaryOfWordModel;
        }

        private static DictionaryQueueItemModel GetVnDictionary(IList<Dictionary> dictionaryOfWord, IList<string>? synonyms, IList<string>? antonyms, Services.DictionaryServices.Models.PhoneticModel? phonetic, string? originPhonetic)
        {
            var dictionaryOfWordModel = dictionaryOfWord.GroupBy(d => d.Word)
                .Select(g => new DictionaryQueueItemModel()
                {
                    Word = g.Key,
                    Type = EnumDictionaryType.EnglishToVietnamese.ToString(),
                    Synonyms = synonyms,
                    Antonyms = antonyms,
                    Phonetic = new Shared.Models.ShareModels.PhoneticModel()
                    {
                        Audio = phonetic?.Audio,
                        Text = phonetic?.Text ?? originPhonetic
                    },
                    PartOfSpeechs = g.ToList().GroupBy(g => g.PartOfSpeech).Select(g => new PartOfSpeechModel()
                    {
                        PartOfSpeech = g.Key,
                        Definitions = g.DistinctBy(d => d.Meaning).ToList().Select(d => new Shared.Models.ShareModels.DefinitionsModel()
                        {
                            Definition = d.Meaning,
                            Examples = d.Examples?.Select(e => new Example()
                            {
                                ExampleVn = e.ExampleVn,
                                ExampleEn = e.ExampleEn
                            }).ToList()
                        }).ToList()
                    }).ToList()
                }).First();

            return dictionaryOfWordModel;
        }
    }
}
