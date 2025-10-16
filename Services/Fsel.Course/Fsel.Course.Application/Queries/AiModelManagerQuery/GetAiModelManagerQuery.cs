// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.AiModelManagerQuery
{
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.CommandModels.AiModelManager;
    using Fsel.Course.Domain.Models.QueryModels.AiModelManager;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetAiModelManagerQuery : SearchAiModelManagerQueryModel, IRequest<MethodResult<PagingItemsModel<AiManagerSearchModel>>>
    {
    }

    public class GetAiModelManagerQueryHandler : IRequestHandler<GetAiModelManagerQuery, MethodResult<PagingItemsModel<AiManagerSearchModel>>>
    {
        private readonly IAiModelManagerRepository _aiModelManagerRepository;

        public GetAiModelManagerQueryHandler(IAiModelManagerRepository aiModelManagerRepository)
        {
            _aiModelManagerRepository = aiModelManagerRepository;
        }

        public async Task<MethodResult<PagingItemsModel<AiManagerSearchModel>>> Handle(GetAiModelManagerQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PagingItemsModel<AiManagerSearchModel>> methodResult = new MethodResult<PagingItemsModel<AiManagerSearchModel>>();
            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var aiModelQuery = _aiModelManagerRepository.Queryable.Where(x => !x.IsDeleted)
                .AsNoTracking()
                .Select(x => new AiManagerSearchModel
                {
                    Id = x.Id,
                    AiModelName = x.AiModelName,
                    CreatedDate = x.CreatedDate,
                    UpdatedDate = x.UpdatedDate,
                });

            request.Keyword = request.Keyword?.Trim().ToLower(CultureInfo.CurrentCulture);
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                if (Guid.TryParse(request.Keyword, out var guid))
                {
                    aiModelQuery = aiModelQuery.Where(m => m.Id == guid);
                }
                else
                {
                    aiModelQuery = aiModelQuery.Where(m => m.AiModelName != null && m.AiModelName.Contains(request.Keyword));
                }
            }

            int totalItem = await aiModelQuery.CountAsync(cancellationToken).ConfigureAwait(false);
            var lists = await aiModelQuery
                .ApplySortAndPaging(request)
                .AsNoTracking()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = new PagingItemsModel<AiManagerSearchModel>(lists, request, totalItem);

            return methodResult;
        }
    }
}
