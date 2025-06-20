// Copyright (c) Atlantic. All rights reserved.

using System.Globalization;
using Fsel.Common.ActionResults;
using Fsel.Core.Base.BaseModels;
using Fsel.Core.Extensions;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Course.Domain.Models.QueryModels.PlacementTests;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Course.Application.Queries.PlacementTestQuery
{
    public class SearchPlacementTestQuery : SearchPlacementTestQueryModel, IRequest<MethodResult<PagingItemsModel<PlacementTestModel>>>
    {
    }

    public class SearchPlacementTestQueryHandler : IRequestHandler<SearchPlacementTestQuery, MethodResult<PagingItemsModel<PlacementTestModel>>>
    {
        private readonly IPlacementTestRepository _placementTestRepository;

        public SearchPlacementTestQueryHandler(IPlacementTestRepository placementTestRepository)
        {
            _placementTestRepository = placementTestRepository;
        }

        public async Task<MethodResult<PagingItemsModel<PlacementTestModel>>> Handle(SearchPlacementTestQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<PlacementTestModel>>();

            if (request.PageSize > 100)
            {
                methodResult.StatusCode = StatusCodes.Status400BadRequest;
                return methodResult;
            }

            var placementTestQuery = from i in _placementTestRepository.Queryable.Where(p => !p.IsArchive)
                                     select new PlacementTestModel
                                     {
                                         Id = i.Id,
                                         Name = i.Name,
                                         InstructionContent = i.InstructionContent,
                                         IsActive = i.IsActive,
                                         Level = i.PlacementTestLevel,
                                         CreatedDate = i.CreatedDate,
                                         CreatedUserId = i.CreatedUserId,
                                         CreatedFullName = i.CreatedFullName,
                                         UpdatedFullName = i.UpdatedFullName,
                                         UpdatedDate = i.UpdatedDate,
                                         UpdatedUserId = i.UpdatedUserId,
                                     };
            //Keyword
            request.Keyword = request.Keyword?.Trim().ToLower(CultureInfo.CurrentCulture);
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                if (Guid.TryParse(request.Keyword, out var guid))
                {
                    placementTestQuery = placementTestQuery.Where(m => m.Id == guid);
                }
                else
                {
                    placementTestQuery = placementTestQuery.Where(m => m.Name != null && m.Name.Contains(request.Keyword));
                }
            }

            if (request.Level != null)
            {
                placementTestQuery = placementTestQuery.Where(m => m.Level == request.Level);
            }

            int totalItem = await placementTestQuery.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await placementTestQuery
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            methodResult.Result = new PagingItemsModel<PlacementTestModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
