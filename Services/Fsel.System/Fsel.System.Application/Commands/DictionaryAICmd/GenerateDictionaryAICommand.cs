// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Commands.DictionaryAICmd
{
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.System.Application.Services.DictionaryServices;
    using global::System;
    using MediatR;

    /// <summary>
    /// Command to generate new dictionary entry via AI (skip cache)
    /// </summary>
    public class GenerateDictionaryAICommand : IRequest<MethodResult<SemanticDictionaryResultModel>>
    {
        public string HighlightedItem { get; set; } = string.Empty;
        public string? SentenceContext { get; set; }
        public string? SourceLanguage { get; set; }
        public string? TargetLanguage { get; set; }
    }

    public class GenerateDictionaryAICommandHandler : IRequestHandler<GenerateDictionaryAICommand, MethodResult<SemanticDictionaryResultModel>>
    {
        private readonly ISemanticDictionaryService _dictionaryService;

        public GenerateDictionaryAICommandHandler(ISemanticDictionaryService dictionaryService)
        {
            _dictionaryService = dictionaryService;
        }

        public async Task<MethodResult<SemanticDictionaryResultModel>> Handle(GenerateDictionaryAICommand request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<SemanticDictionaryResultModel>();

            if (string.IsNullOrWhiteSpace(request.HighlightedItem))
            {
                methodResult.AddError("INVALID_INPUT", "Highlighted item is required");
                return methodResult;
            }

            var semanticRequest = new SemanticDictionaryRequestModel
            {
                HighlightedItem = request.HighlightedItem,
                SentenceContext = request.SentenceContext,
                SourceLanguage = request.SourceLanguage ?? "en",
                TargetLanguage = request.TargetLanguage ?? "vi"
            };

            var result = await _dictionaryService.GenerateAsync(semanticRequest, cancellationToken);
            methodResult.Result = result;
            methodResult.StatusCode = 200;

            return methodResult;
        }
    }
}
