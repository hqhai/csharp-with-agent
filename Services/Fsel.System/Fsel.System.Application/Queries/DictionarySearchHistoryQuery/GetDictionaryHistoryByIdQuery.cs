// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.DictionarySearchHistoryQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.System.Infrastructure;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetDictionaryHistoryByIdQuery : IRequest<MethodResult<SemanticDictionaryResultModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetDictionaryHistoryByIdQueryHandler : IRequestHandler<GetDictionaryHistoryByIdQuery, MethodResult<SemanticDictionaryResultModel>>
    {
        private readonly PostgreDbContext _context;

        public GetDictionaryHistoryByIdQueryHandler(PostgreDbContext context)
        {
            _context = context;
        }

        public async Task<MethodResult<SemanticDictionaryResultModel>> Handle(
            GetDictionaryHistoryByIdQuery request,
            CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<SemanticDictionaryResultModel>();

            var history = await _context.DictionarySearchHistories
                .Where(h => h.Id == request.Id && !h.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);

            if (history == null)
            {
                methodResult.AddError("NOT_FOUND", "Search history not found");
                return methodResult;
            }

            if (history.DictionaryAIId == null)
            {
                methodResult.AddError("NOT_FOUND", "No dictionary entry found for this history");
                return methodResult;
            }

            var dictionaryAI = await _context.DictionaryAIs
                .Where(d => d.Id == history.DictionaryAIId && !d.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);

            if (dictionaryAI == null)
            {
                methodResult.AddError("NOT_FOUND", "Dictionary entry not found");
                return methodResult;
            }

            var result = new SemanticDictionaryResultModel
            {
                Id = dictionaryAI.Id.ToString(),
                HighlightedItemSource = dictionaryAI.HighlightedItemSource,
                HighlightedItemTarget = dictionaryAI.HighlightedItemTarget,
                DefinitionSource = dictionaryAI.DefinitionSource,
                DefinitionTarget = dictionaryAI.DefinitionTarget,
                JsonPayload = dictionaryAI.JsonPayload,
                ExampleSentenceSource = dictionaryAI.ExampleSentenceSource,
                ExampleSentenceTarget = dictionaryAI.ExampleSentenceTarget,
                SourceLanguage = dictionaryAI.SourceLanguage,
                TargetLanguage = dictionaryAI.TargetLanguage,
                HasAudioFromLegacy = dictionaryAI.HasAudioFromLegacy,
                IsFromCache = true
            };

            methodResult.Result = result;
            return methodResult;
        }
    }
}
