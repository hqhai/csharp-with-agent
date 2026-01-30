// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.DictionaryAICmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.System.Application.Services.DictionaryServices;
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

        public SearchDictionaryAICommandHandler(ISemanticDictionaryService dictionaryService)
        {
            _dictionaryService = dictionaryService;
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

            var semanticRequest = new SemanticDictionaryRequestModel
            {
                HighlightedItem = request.HighlightedItem,
                SentenceContext = request.SentenceContext,
                SourceLanguage = request.SourceLanguage ?? "en",
                TargetLanguage = request.TargetLanguage ?? "vi"
            };

            var result = await _dictionaryService.SearchAsync(semanticRequest, cancellationToken);
            if (result != null)
            {
                methodResult.Result = result;
                methodResult.StatusCode = 200;
            }

            return methodResult;
        }
    }
}
