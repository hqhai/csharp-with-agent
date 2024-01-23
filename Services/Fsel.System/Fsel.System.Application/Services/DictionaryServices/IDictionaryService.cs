// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Services.DictionaryServices
{
    using Fsel.Common.ActionResults;
    using Fsel.System.Application.Services.DictionaryServices.Models;
    using Microsoft.AspNetCore.Mvc;
    using Refit;

    public interface IDictionaryService
    {
        [Get("/entries/en/{word}")]
        Task<IApiResponse<IList<DictionaryModel>>> GetDictionaryByWordAsync([FromQuery] string word);
    }
}
