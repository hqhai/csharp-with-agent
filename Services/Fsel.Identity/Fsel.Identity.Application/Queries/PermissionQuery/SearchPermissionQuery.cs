using Fsel.Common.ActionResults;
using Fsel.Core.Base.BaseModels;
using Fsel.Core.Extensions;
using Fsel.Identity.Domain.IRepositories;
using Fsel.Identity.Domain.Models.EntityModels.Permissions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Identity.Application.Queries.PermissionQuery
{
    public class SearchPermissionQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<PermissionModel>>>
    {
    }

    public class SearchPermissionQueryHandler : IRequestHandler<SearchPermissionQuery, MethodResult<PagingItemsModel<PermissionModel>>>
    {
        private readonly IPermissionRepository _permissionRepository;
        private readonly IPermissionGroupRepository _permissionGroupRepository;

        public SearchPermissionQueryHandler(IPermissionRepository permissionRepository, IPermissionGroupRepository permissionGroupRepository)
        {
            _permissionRepository = permissionRepository;
            _permissionGroupRepository = permissionGroupRepository;
        }

        public async Task<MethodResult<PagingItemsModel<PermissionModel>>> Handle(SearchPermissionQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<PermissionModel>>();

            var query = from p in _permissionRepository.Queryable
                        join pg in _permissionGroupRepository.Queryable on p.PermissionGroupId equals pg.Id
                        select new PermissionModel()
                        {
                            Id = p.Id,
                            ClaimValue = p.ClaimValue,
                            Name = p.Name,
                            Description = p.Description,
                            Status = p.Status,
                            CreatedDate = p.CreatedDate,
                            PermissionGroupName = pg.Name,
                            PermissionGroupId = p.PermissionGroupId
                        };

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                query = query.Where(m => (m.ClaimValue != null && m.ClaimValue.Contains(request.Keyword)) || (m.Name != null && m.Name.Contains(request.Keyword)));
            }

            int totalItem = await query.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await query
                     .ApplySortAndPaging(request)
                     .AsNoTracking()
                     .ToListAsync(cancellationToken: cancellationToken)
                     .ConfigureAwait(false);

            methodResult.Result = new PagingItemsModel<PermissionModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
