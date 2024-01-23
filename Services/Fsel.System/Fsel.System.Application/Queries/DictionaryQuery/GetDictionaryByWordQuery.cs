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
    using Microsoft.EntityFrameworkCore;

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

            UpdateResultDictionaryAsync(result, x => x.Word!, (word, value) => word.Word = value);
            UpdateResultDictionaryAsync(result, x => x.Phonetic!, (word, value) => word.Phonetic = value);
            UpdateResultDictionaryAsync(result, x => x.Origin!, (word, value) => word.Origin = value);

            //phonetics
            var phonetics = result.SelectMany(x => x.Phonetics!);

            UpdatePropertiesIfForbidden(phonetics, x => x.Text!, (p, value) => p.Text = value);
            UpdatePropertiesIfForbidden(phonetics, x => x.Audio!, (p, value) => p.Audio = value);

            //license
            var license = result.Select(x => x.License);
            if (license != null)
            {
                UpdatePropertiesIfForbidden(license, x => x.Name!, (p, value) => p.Name = value);
                UpdatePropertiesIfForbidden(license, x => x.Url!, (p, value) => p.Url = value);
            }

            //meanings
            var meanings = result.SelectMany(x => x.Meanings!);

            UpdatePropertiesIfForbidden(meanings, x => x.PartOfSpeech!, (p, value) => p.PartOfSpeech = value);

            UpdatePropertiesIfForbiddenType2(meanings, _forbiddenWordRepository.Queryable, x => x.Synonyms, (p, value) => p.Synonyms = value);
            UpdatePropertiesIfForbiddenType2(meanings, _forbiddenWordRepository.Queryable, x => x.Antonyms, (p, value) => p.Antonyms = value);

            var definitions = meanings.SelectMany(x => x.Definitions!);
            UpdatePropertiesIfForbiddenType1(definitions, _forbiddenWordRepository.Queryable, x => x.Definition, (p, value) => p.Definition = value);
            UpdatePropertiesIfForbiddenType1(definitions, _forbiddenWordRepository.Queryable, x => x.Example, (p, value) => p.Example = value);

            methodResult.Result = result;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private void UpdateResultDictionaryAsync(IEnumerable<DictionaryModel> result, Func<DictionaryModel, string> propertySelector, Action<DictionaryModel, string> propertyUpdater)
        {
            if (_forbiddenWordRepository.Queryable.Any(x => result.Select(propertySelector).Contains(x.Word)))
            {
                foreach (var word in result)
                {
                    propertyUpdater(word, string.Empty);
                }
            }
        }

        public void UpdatePropertiesIfForbidden<T>(IEnumerable<T> items, Func<T, string> propertySelector, Action<T, string?> propertyUpdater)
        {
            if (items != null && propertyUpdater != null)
            {
                if (_forbiddenWordRepository.Queryable.Any(x => items.Select(propertySelector).Contains(x.Word)))
                {
                    foreach (var item in items)
                    {
                        propertyUpdater(item, null);
                    }
                }
            }
        }

        public void UpdatePropertiesIfForbiddenType1<T>(IEnumerable<T> items, IQueryable<ForbiddenWord> forbiddenWord, Func<T, string?> propertySelector, Action<T, string?> propertyUpdater)
        {
            if (items != null && propertySelector != null)
            {
                foreach (var item in items)
                {
                    var propertyValue = propertySelector(item);

                    if (propertyValue != null && forbiddenWord.Any(x => propertyValue.Contains(x.Word!)) && propertyUpdater != null)
                    {
                        propertyUpdater(item, null);
                    }
                }
            }
        }

        public void UpdatePropertiesIfForbiddenType2<T>(IEnumerable<T> items, IQueryable<ForbiddenWord> forbiddenWord, Func<T, IList<string>?> propertySelector, Action<T, IList<string>?> propertyUpdater)
        {
            if (items != null && propertySelector != null)
            {
                foreach (var item in items)
                {
                    var propertyValue = propertySelector(item);

                    if (propertyValue != null && forbiddenWord.Any(x => propertyValue.Contains(x.Word!)) && propertyUpdater != null)
                    {
                        propertyUpdater(item, null);
                    }
                }
            }
        }
    }
}
