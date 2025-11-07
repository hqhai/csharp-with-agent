// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.DocumentQuery
{
    using AutoMapper;
    using Common.ActionResults;
    using Common.Enums;
    using Common.Enums.ErrorCodes;
    using Domain.IRepositories;
    using Domain.Models.EntityModels.V1i1;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Services.ApplicationServices.CacheServices;

    public class SearchDocumentQuery : IRequest<MethodResult<DocumentModel>>
    {
        public Guid OriginalId { get; set; }
    }

    public class SearchDocumentQueryHandler : IRequestHandler<SearchDocumentQuery, MethodResult<DocumentModel>>
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IDocumentCachingService _documentCachingService;
        private readonly IMapper _mapper;

        public SearchDocumentQueryHandler(
            IDocumentRepository documentRepository,
            IDocumentCachingService documentCachingService,
            IMapper mapper)
        {
            _documentRepository = documentRepository;
            _documentCachingService = documentCachingService;
            _mapper = mapper;
        }

        public async Task<MethodResult<DocumentModel>> Handle(SearchDocumentQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<DocumentModel>();

            if (request.OriginalId == Guid.Empty)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required));
                return methodResult;
            }

            string cacheKey = $"DocumentQuery_{request.OriginalId}";

            var documentResult =  await _documentCachingService.GetOrSetAsync(cacheKey, async (ctx, _) =>
            {
                var document = await _documentRepository.ReadQueryable
                    .Where(x => x.OriginalId == request.OriginalId && x.VersionStatus == EnumVersionStatus.LastVersion)
                    .AsNoTracking()
                    .SingleOrDefaultAsync(cancellationToken);

                return document ?? null!;
            }, token: cancellationToken);

            methodResult.Result = _mapper.Map<DocumentModel>(documentResult);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
