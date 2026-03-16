// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.DictionaryAICmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Infrastructure;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.System.Application.Services.DictionaryServices;
    using global::System.Diagnostics;
    using MediatR;


    public class SearchDictionaryAICommand : IRequest<MethodResult<SemanticDictionaryResultModel>>
    {
        public string HighlightedItem { get; set; } = string.Empty;
        public string? SentenceContext { get; set; }
        public string? SourceLanguage { get; set; }
        public string? TargetLanguage { get; set; }
    }

    public class SearchDictionaryAICommandHandler : IRequestHandler<SearchDictionaryAICommand, MethodResult<SemanticDictionaryResultModel>>
    {
        private readonly ISemanticDictionaryService _dictionaryService;
        private readonly PostgreDbContext _context;
        private readonly AuthContext _authContext;

        public SearchDictionaryAICommandHandler(
            ISemanticDictionaryService dictionaryService,
            PostgreDbContext context,
            AuthContext authContext)
        {
            _dictionaryService = dictionaryService;
            _context = context;
            _authContext = authContext;
        }

        public async Task<MethodResult<SemanticDictionaryResultModel>> Handle(SearchDictionaryAICommand request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<SemanticDictionaryResultModel>();

            if (string.IsNullOrWhiteSpace(request.HighlightedItem))
            {
                methodResult.AddError("INVALID_INPUT", "Highlighted item is required");
                return methodResult;
            }

            if (request.HighlightedItem.Length > 240)
            {
                methodResult.AddError("LENGTH_EXCEEDED", "Highlighted item exceeds 240 characters");
                return methodResult;
            }

            var stopwatch = Stopwatch.StartNew();
            var semanticRequest = new SemanticDictionaryRequestModel
            {
                HighlightedItem = request.HighlightedItem,
                SentenceContext = request.SentenceContext,
                SourceLanguage = request.SourceLanguage ?? "en",
                TargetLanguage = request.TargetLanguage ?? "vi"
            };

            var result = await _dictionaryService.SearchAsync(semanticRequest, cancellationToken);
            stopwatch.Stop();

            // Save search history
            if (_authContext.CurrentUserId != Guid.Empty)
            {
                var searchHistory = new DictionarySearchHistory
                {
                    Id = Guid.NewGuid(),
                    UserId = _authContext.CurrentUserId,
                    SearchTerm = request.HighlightedItem,
                    SearchContext = request.SentenceContext,
                    SourceLanguage = semanticRequest.SourceLanguage,
                    TargetLanguage = semanticRequest.TargetLanguage,
                    FoundResult = result != null,
                    ResultCount = result != null ? 1 : 0,
                    ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                    CreatedDate = DateTime.UtcNow,
                    CreatedUserId = _authContext.CurrentUserId,
                    CreatedFullName = _authContext.CurrentFullName ?? _authContext.CurrentUsername ?? "Unknown"
                };

                if (result != null && !string.IsNullOrEmpty(result.Id))
                {
                    searchHistory.DictionaryAIId = Guid.TryParse(result.Id, out var dictId) ? dictId : null;
                }

                _context.DictionarySearchHistories.Add(searchHistory);
                await _context.SaveChangesAsync(cancellationToken);
            }

            if (result != null)
            {
                methodResult.Result = result;
                methodResult.StatusCode = 200;
            }

            return methodResult;
        }
    }
}
