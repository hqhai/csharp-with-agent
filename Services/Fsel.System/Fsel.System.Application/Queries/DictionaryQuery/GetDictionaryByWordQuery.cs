// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.DictionaryQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.System.Application.Services.DictionaryServices;
    using Fsel.System.Application.Services.DictionaryServices.Models;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.IRepositories;
    using global::System.Collections;
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

            if (forbiddenWord.Any(x => result.Select(x => x.Word).Contains(x.Word)))
            {
                foreach (var word in result)
                {
                    word.Word = null;
                }
            }

            if (forbiddenWord.Any(x => result.Select(x => x.Phonetic).Contains(x.Word)))
            {
                foreach (var word in result)
                {
                    word.Phonetics = null;
                }
            }

            var phonetics = result.SelectMany(x => x.Phonetics!);

            if (forbiddenWord.Any(x => phonetics.Select(x => x.Text).Contains(x.Word)))
            {
                foreach (var p in phonetics)
                {
                    p.Text = null;
                }
            }

            if (forbiddenWord.Any(x => phonetics.Select(x => x.Audio).Contains(x.Word)))
            {
                foreach (var p in phonetics)
                {
                    p.Audio = null;
                }
            }

            if (forbiddenWord.Any(x => result.Select(x => x.Origin).Contains(x.Word)))
            {
                foreach (var word in result)
                {
                    word.Origin = null;
                }
            }

            var license = result.Select(x => x.License);
            if (license != null)
            {
                if (forbiddenWord.Any(x => license.Select(x => x!.Name).Contains(x.Word)))
                {
                    foreach (var p in license)
                    {
                        p!.Name = null;
                    }
                }
                if (forbiddenWord.Any(x => license.Select(x => x!.Url).Contains(x.Word)))
                {
                    foreach (var p in license)
                    {
                        p!.Url = null;
                    }
                }
            }

            var meanings = result.SelectMany(x => x.Meanings!);

            if (forbiddenWord.Any(x => meanings.Select(x => x.PartOfSpeech).Contains(x.Word)))
            {
                foreach (var p in meanings)
                {
                    p.PartOfSpeech = null;
                }
            }

            var definitions = meanings.SelectMany(x => x.Definitions!);

            foreach (var p in definitions)
            {
                if (p.Definition != null && forbiddenWord.Any(x => p.Definition.Contains(x.Word!)))
                {
                    p.Definition = null;
                }

                if (p.Example != null && forbiddenWord.Any(x => p.Example.Contains(x.Word!)))
                {
                    p.Example = null;
                }
            }

            foreach (var p in meanings)
            {
                if (p.Synonyms != null && forbiddenWord.Any(x => p.Synonyms.Contains(x.Word!)))
                {
                    p.Synonyms = null;
                }
                if (p.Antonyms != null && forbiddenWord.Any(x => p.Antonyms.Contains(x.Word!)))
                {
                    p.Antonyms = null;
                }
            }

            methodResult.Result = result;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<object?> loop(object? data, string? nameProperty)
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
                            word. = null;
                        }
                    }
                }
            }
            return data;
        }
    }
}
