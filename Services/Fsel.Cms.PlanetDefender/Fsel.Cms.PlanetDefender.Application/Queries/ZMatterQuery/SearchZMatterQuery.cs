// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Cms.PlanetDefender.Application.Queries.ZMatterQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Cms.PlanetDefender.Application.Queries.StudentGameInfoQuery;
    using Fsel.Cms.PlanetDefender.Domain.IRepositories;
    using Fsel.Cms.PlanetDefender.Domain.Models.EntityModels;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class SearchZMatterQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<ZMatterModel>>>
    {
    }
    public class SearchZMatterQueryHandler : IRequestHandler<SearchZMatterQuery, MethodResult<PagingItemsModel<ZMatterModel>>>
    {
        private readonly IZMatterRepository _zMatterRepository;

        public SearchZMatterQueryHandler(IZMatterRepository zMatterRepository)
        {
            _zMatterRepository = zMatterRepository;
        }

        public async Task<MethodResult<PagingItemsModel<ZMatterModel>>> Handle(SearchZMatterQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<ZMatterModel>>();

            var zMatter = _zMatterRepository.Queryable
                            .Select(x => new ZMatterModel
                            {
                                Id = x.Id,
                                Code = x.Code,
                                CreatedDate = x.CreatedDate,
                                CreatedFullName = x.CreatedFullName,
                                Description = x.Description,
                                FilePath = x.FilePath,
                                IsActive = x.IsActive,
                                Name = x.Name,
                                Usage = x.Usage,
                            });
            int totalItem = await zMatter.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await zMatter
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            methodResult.Result = new PagingItemsModel<ZMatterModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
