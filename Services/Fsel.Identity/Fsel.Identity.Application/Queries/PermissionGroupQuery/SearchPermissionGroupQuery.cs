using Fsel.Common.ActionResults;
using Fsel.Core.Base.BaseModels;
using Fsel.Core.Extensions;
using Fsel.Identity.Domain.IRepositories;
using Fsel.Identity.Domain.Models.EntityModels.Permissions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Fsel.Identity.Application.Queries.PermissionGroupQuery
{
    public class SearchPermissionGroupQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<PermissionGroupModel>>>
    {
    }

    public class SearchPermissionGroupQueryHandler : IRequestHandler<SearchPermissionGroupQuery, MethodResult<PagingItemsModel<PermissionGroupModel>>>
    {
        private readonly IPermissionGroupRepository _permissionGroupRepository;

        public SearchPermissionGroupQueryHandler(IPermissionGroupRepository permissionGroupRepository)
        {
            _permissionGroupRepository = permissionGroupRepository;
        }

        public async Task<MethodResult<PagingItemsModel<PermissionGroupModel>>> Handle(SearchPermissionGroupQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<PermissionGroupModel>>();

            var query = _permissionGroupRepository.Queryable.Include(p => p.Permissions).Select(p => new PermissionGroupModel()
            {
                Id = p.Id,
                ClaimType = p.ClaimType,
                Name = p.Name,
                Description = p.Description,
                Status = p.Status,
                CreatedDate = p.CreatedDate,
                MenuId = p.MenuId,
                Permissions = p.Permissions.Select(x => new PermissionModel()
                {
                    Id = x.Id,
                    Name = x.Name,
                    ClaimValue = x.ClaimValue,
                    Description = x.Description,
                    PermissionGroupId = x.PermissionGroupId,
                    Status = x.Status,
                    CreatedDate = x.CreatedDate
                }
                ).ToList()
            });

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                query = query.Where(m => (m.ClaimType != null && m.ClaimType.Contains(request.Keyword)) || (m.Name != null && m.Name.Contains(request.Keyword)));
            }

            int totalItem = await query.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await query
                     .ApplySortAndPaging(request)
                     .AsNoTracking()
                     .ToListAsync(cancellationToken: cancellationToken)
                     .ConfigureAwait(false);

            methodResult.Result = new PagingItemsModel<PermissionGroupModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
