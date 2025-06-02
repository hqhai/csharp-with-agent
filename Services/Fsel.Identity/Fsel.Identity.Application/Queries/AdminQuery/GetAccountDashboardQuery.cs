// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.AdminQuery
{
    using System.Linq;
    using System.Text.Json.Serialization;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetAccountDashboardQuery : BaseQueryModel, IRequest<MethodResult<PagingItemsModel<GetAccountDashboardQueryModel>>>
    {
        public IList<string>? Roles { get; set; }

        [JsonIgnore]
        public IList<EnumRole>? ListRoles => Roles.ToList<EnumRole>();

        public IList<string>? EventCodes { get; set; }
    }

    public class GetAccountDashboardQueryHandler : IRequestHandler<GetAccountDashboardQuery, MethodResult<PagingItemsModel<GetAccountDashboardQueryModel>>>
    {
        private readonly IUserRoleRepository _userRoleRepository;

        public GetAccountDashboardQueryHandler(IUserRoleRepository userRoleRepository)
        {
            _userRoleRepository = userRoleRepository;
        }

        public async Task<MethodResult<PagingItemsModel<GetAccountDashboardQueryModel>>> Handle(GetAccountDashboardQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PagingItemsModel<GetAccountDashboardQueryModel>> methodResult = new MethodResult<PagingItemsModel<GetAccountDashboardQueryModel>>();

            IList<EnumRole> roles = new List<EnumRole>();

            if (request.ListRoles == null || !request.ListRoles.Any())
            {
                roles.Add(EnumRole.DepartmentAdmin);
                roles.Add(EnumRole.EducationDepartment);
                roles.Add(EnumRole.EducationDivision);
            }
            else
            {
                roles = request.ListRoles;
            }

            var users = _userRoleRepository.GetUsersByRoles(roles);

            if (request.EventCodes != null)
            {
                users = users.Where(x => x.EventCode != null && request.EventCodes.Contains(x.EventCode));
            }

            if (!string.IsNullOrEmpty(request.Keyword))
            {
                users = users.Where(x => (!string.IsNullOrEmpty(x.UserName) && x.UserName.Trim().Contains(request.Keyword.Trim())) ||
                                         (!string.IsNullOrEmpty(x.FullName) && x.FullName.Trim().Contains(request.Keyword.Trim())));
            }

            int totalItem = await users.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await users.ApplySortAndPaging(request)
                                   .AsNoTracking()
                                   .ToListAsync(cancellationToken: cancellationToken)
                                   .ConfigureAwait(false);

            methodResult.Result = new PagingItemsModel<GetAccountDashboardQueryModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
