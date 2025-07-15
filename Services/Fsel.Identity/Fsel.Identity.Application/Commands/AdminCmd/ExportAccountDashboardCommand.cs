// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Commands.AdminCmd
{
    using System.Linq;
    using System.Text.Json.Serialization;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.CommandModels.Admins;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class ExportAccountDashboardCommand : IRequest<MethodResult<Stream>>
    {
        public string? Keyword { get; set; }

        public IList<string>? Roles { get; set; }

        [JsonIgnore]
        public IList<EnumRole>? ListRoles => Roles.ToList<EnumRole>();

        public IList<string>? EventCodes { get; set; }

        public IList<Guid>? UserIds { get; set; }
    }

    public class ExportAccountDashboardCommandHandler : IRequestHandler<ExportAccountDashboardCommand, MethodResult<Stream>>
    {
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IMapper _mapper;
        private const string DistrictCity = "District&City";
        private const string City = "City";
        private const string District = "District";

        public ExportAccountDashboardCommandHandler(IUserRoleRepository userRoleRepository,
                                                    IMapper mapper)
        {
            _userRoleRepository = userRoleRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<Stream>> Handle(ExportAccountDashboardCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<Stream>();

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

            if (request.UserIds != null)
            {
                users = users.Where(x => request.UserIds.Contains(x.Id));
            }

            var userResults = await users.ToListAsync(cancellationToken);
            if (userResults == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var template = _mapper.Map<IList<ExportAccountDashboardCommandModel>>(userResults);
            foreach (var item in template)
            {
                if (item.Role == EnumRole.DepartmentAdmin.ToString())
                {
                    item.Role = DistrictCity;
                }
                else if (item.Role == EnumRole.EducationDepartment.ToString())
                {
                    item.Role = City;
                }
                else if (item.Role == EnumRole.EducationDivision.ToString())
                {
                    item.Role = District;
                }
            }
            methodResult.Result = template.ExportExcel();
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
