// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.DictionaryQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.System.Application.Services.DictionaryServices;
    using Fsel.System.Application.Services.DictionaryServices.Models;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.IRepositories;
    using global::System.Collections.Generic;
    using global::System.Linq;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetDictionaryByWordQuery : IRequest<MethodResult<IList<DictionaryModel>>>
    {
        public string? Word { get; set; }
    }

    public class GetDictionaryByWordQueryHandler : IRequestHandler<GetDictionaryByWordQuery, MethodResult<IList<DictionaryModel>>>
    {
        private readonly IDictionaryService _dictionaryService;
        private readonly IForbiddenWordRepository _forbiddenWordRepository;

        public GetDictionaryByWordQueryHandler(IDictionaryService dictionaryService, IForbiddenWordRepository forbiddenWordRepository)
        {
            _dictionaryService = dictionaryService;
            _forbiddenWordRepository = forbiddenWordRepository;
        }

        public async Task<MethodResult<IList<DictionaryModel>>> Handle(GetDictionaryByWordQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<DictionaryModel>> methodResult = new MethodResult<IList<DictionaryModel>>();

            var responseResult = await _dictionaryService.GetDictionaryByWordAsync(request.Word);
            var result = responseResult.Content;

            if (result == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(result));
                return methodResult;
            }

            var forbiddenWord = _forbiddenWordRepository.Queryable;

            foreach (var word in result)
            {
                ProcessForbiddenWord(word, forbiddenWord);
            }

            methodResult.Result = result;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;

            // Helper method to process forbidden words for a single word entry
            void ProcessForbiddenWord(DictionaryModel word, IQueryable<ForbiddenWord> forbiddenWords)
            {
                void SetToNullIfForbidden(Func<DictionaryModel, string?> propertySelector)
                {
                    var propertyValue = propertySelector(word);
                    if (propertyValue != null && forbiddenWords.Any(x => x.Word == propertyValue))
                    {
                        propertySelector(word) = null;
                    }
                }

                SetToNullIfForbidden(w => w.Word);
                SetToNullIfForbidden(w => w.Phonetic);
                SetToNullIfForbidden(w => w.Origin);
                SetToNullIfForbidden(w => w.License?.Name);
                SetToNullIfForbidden(w => w.License?.Url);

                var phonetics = word.Phonetics;
                if (phonetics != null)
                {
                    foreach (var phonetic in phonetics)
                    {
                        SetToNullIfForbidden(p => p.Text);
                        SetToNullIfForbidden(p => p.Audio);
                    }
                }

                var meanings = word.Meanings;
                if (meanings != null)
                {
                    foreach (var meaning in meanings)
                    {
                        if (forbiddenWords.Any(x => x.Word == meaning.PartOfSpeech))
                        {
                            meaning.PartOfSpeech = null;
                        }

                        foreach (var definition in meaning.Definitions ?? Enumerable.Empty<DefinitionsModel>())
                        {
                            SetToNullIfForbidden(d => d.Definition);
                            SetToNullIfForbidden(d => d.Example);
                        }

                        SetToNullIfForbidden(m => m.Synonyms);
                        SetToNullIfForbidden(m => m.Antonyms);
                    }
                }
            }
        }

        /* private async Task<object?> loop(object? data, string? nameProperty)
         {
             var query = _forbiddenWordRepository.Queryable;
             if (data is IList list)
             {
                 var objects = list.Cast<object>().ToList();
                 if (objects != null && objects.Any())
                 {
                     if (await query.AnyAsync(x => objects.Select(x => x.GetPropValue<string>(nameProperty)).Contains(x.Word)))
                     {
                         foreach (var word in objects)
                         {
                             word.GetPropValue<string>(nameProperty) = null;
                         }
                     }
                 }
             }
             return data;
         }*/
    }
}
