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

    public class GetDocumentQuery : IRequest<MethodResult<DocumentModel>>
    {
        public Guid DocumentId { get; set; }
    }

    public class GetDocumentQueryHandler : IRequestHandler<GetDocumentQuery, MethodResult<DocumentModel>>
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IDocumentCachingService _documentCachingService;
        private readonly IMapper _mapper;

        public GetDocumentQueryHandler(
            IDocumentRepository documentRepository,
            IDocumentCachingService documentCachingService,
            IMapper mapper)
        {
            _documentRepository = documentRepository;
            _documentCachingService = documentCachingService;
            _mapper = mapper;
        }

        public async Task<MethodResult<DocumentModel>> Handle(GetDocumentQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<DocumentModel>();

            string cacheKey = $"DocumentQuery_{request.DocumentId}";

            var documentResult =  await _documentCachingService.GetOrSetAsync(cacheKey, async (ctx, _) =>
            {
                var document = await _documentRepository.ReadQueryable
                    .Where(x => x.Id == request.DocumentId && x.VersionStatus == EnumVersionStatus.LastVersion)
                    .SingleOrDefaultAsync(cancellationToken);
                return document ?? null!;
            }, token: cancellationToken);

            methodResult.Result = _mapper.Map<DocumentModel>(documentResult);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
