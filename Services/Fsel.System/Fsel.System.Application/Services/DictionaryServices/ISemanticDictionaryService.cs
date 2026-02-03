// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Services.DictionaryServices
{
    using Fsel.Shared.Models.ShareModels;
    public interface ISemanticDictionaryService
    {

        Task<SemanticDictionaryResultModel?> SearchAsync(
            SemanticDictionaryRequestModel request,
            CancellationToken cancellationToken = default);


        Task<SemanticDictionaryResultModel> GenerateAsync(
            SemanticDictionaryRequestModel request,
            CancellationToken cancellationToken = default);


        Task<SemanticDictionaryResultModel?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);


        Task<float[]> GenerateEmbeddingAsync(
            string text,
            CancellationToken cancellationToken = default);


        Task<Models.AIResponseModel> CallAIAsync(
            SemanticDictionaryRequestModel request,
            CancellationToken cancellationToken = default);
    }
}
