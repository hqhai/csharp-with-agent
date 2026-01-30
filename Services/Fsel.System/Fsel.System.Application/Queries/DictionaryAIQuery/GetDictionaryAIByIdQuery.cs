// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.DictionaryAIQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Shared.Models.ShareModels;
    using Fsel.System.Application.Services.DictionaryServices;
    using global::System;
    using MediatR;

    /// <summary>
    /// Query to get dictionary entry by ID
    /// </summary>
    public class GetDictionaryAIByIdQuery : IRequest<MethodResult<SemanticDictionaryResultModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetDictionaryAIByIdQueryHandler : IRequestHandler<GetDictionaryAIByIdQuery, MethodResult<SemanticDictionaryResultModel>>
    {
        private readonly ISemanticDictionaryService _dictionaryService;

        public GetDictionaryAIByIdQueryHandler(ISemanticDictionaryService dictionaryService)
        {
            _dictionaryService = dictionaryService;
        }

        public async Task<MethodResult<SemanticDictionaryResultModel>> Handle(GetDictionaryAIByIdQuery request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<SemanticDictionaryResultModel>();

            if (request.Id == Guid.Empty)
            {
                methodResult.AddError("INVALID_ID", "Invalid dictionary ID");
                return methodResult;
            }

            var result = await _dictionaryService.GetByIdAsync(request.Id, cancellationToken);

            if (result == null)
            {
                methodResult.AddError("NOT_FOUND", "Dictionary entry not found");
                return methodResult;
            }

            methodResult.Result = result;
            methodResult.StatusCode = 200;
            return methodResult;
        }
    }
}
