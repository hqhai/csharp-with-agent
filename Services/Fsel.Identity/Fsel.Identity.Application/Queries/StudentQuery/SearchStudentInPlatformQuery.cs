// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.StudentQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.BaseModels;
    using Fsel.Core.Extensions;
    using Fsel.Identity.Application.Services.CMSPlanetDefenderService;
    using Fsel.Identity.Application.Services.CMSPlanetDefenderService.Models;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Identity.Domain.Models.QueryModels.Students;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;

    public class SearchStudentInPlatformQuery : SearchStudentInPlatformQueryModel, IRequest<MethodResult<PagingItemsModel<StudentInPlatformModel>>>
    {
    }

    public class SearchStudentInPlatformQueryHandler : IRequestHandler<SearchStudentInPlatformQuery, MethodResult<PagingItemsModel<StudentInPlatformModel>>>
    {
        private readonly IPlatformRepository _platformRepository;
        private readonly IUserPlatformRepository _userPlatformRepository;
        private readonly UserManager<User> _userManager;
        private readonly IHumanRepository _humanRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly ICMSPlanetDefenderService _cmsPlanetDefenderService;
        private readonly RoleManager<Role> _roleManager;
        private readonly IUserRoleRepository _userRoleRepository;

        public SearchStudentInPlatformQueryHandler(IPlatformRepository platformRepository, IUserPlatformRepository userPlatformRepository, UserManager<User> userManager, IHumanRepository humanRepository, IStudentRepository studentRepository, ICMSPlanetDefenderService cmsPlanetDefenderService, RoleManager<Role> roleManager, IUserRoleRepository userRoleRepository)
        {
            _platformRepository = platformRepository;
            _userPlatformRepository = userPlatformRepository;
            _userManager = userManager;
            _humanRepository = humanRepository;
            _studentRepository = studentRepository;
            _cmsPlanetDefenderService = cmsPlanetDefenderService;
            _roleManager = roleManager;
            _userRoleRepository = userRoleRepository;
        }

        public async Task<MethodResult<PagingItemsModel<StudentInPlatformModel>>> Handle(SearchStudentInPlatformQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PagingItemsModel<StudentInPlatformModel>>();

            Guid? platformId = null;
            if (request.PlatformCode.HasValue)
            {
                var platform = await _platformRepository.Queryable.FirstOrDefaultAsync(p => p.Code == request.PlatformCode, cancellationToken);
                platformId = platform?.Id;
            }
            var userRoleQuery = _userRoleRepository.GetQuery();
            var query = from a in _userPlatformRepository.Queryable.Where(n => !platformId.HasValue || n.PlatformId == platformId)
                        join b in _userManager.Users on a.UserId equals b.Id
                        join c in _humanRepository.Queryable on b.Id equals c.UserId
                        join d in _studentRepository.Queryable on c.Id equals d.HumanId
                        join ur in userRoleQuery on b.Id equals ur.UserId
                        join r in _roleManager.Roles on ur.RoleId equals r.Id
                        select new StudentInPlatformModel
                        {
                            Id = a.Id,
                            Code = c.Code,
                            UserName = b.UserName,
                            Role = r.Name,
                            StudentId = d.Id,
                            UserPlatformStatus = a.Status,
                            CreatedDate = a.CreatedDate,
                        };
            query = query.Where(p => p.UserPlatformStatus == request.Status);
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                query = query.Where(p => (!string.IsNullOrEmpty(p.UserName) && p.UserName.Contains(request.Keyword)) || (!string.IsNullOrEmpty(p.Code) && p.Code.Contains(request.Keyword)));
            }
            var getLevelOfStudentsResult = await _cmsPlanetDefenderService.GetLevelOfStudentByStudentIds(new GetLevelOfStudentsByStudentIdsQueryModel
            {
                StudentIds = query.Select(p => p.StudentId).Distinct().ToList(),
            });
            var getLevelOfStudents = getLevelOfStudentsResult.Content?.Result;
            if (request.Level.HasValue)
            {
                var levelOfStudents = getLevelOfStudents?.Where(p => p.Level.HasValue && p.Level == request.Level).ToList();
                query = query.Where(p => levelOfStudents != null && levelOfStudents.Select(x => x.StudentId).Contains(p.StudentId));
            }
            if (request.Role.HasValue)
            {
                query = query.Where(p => p.Role == request.Role.ToString());
            }
            int totalItem = await query.CountAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lists = await query
                    .ApplySortAndPaging(request)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            lists.ForEach(x =>
            {
                x.Level = getLevelOfStudents?.FirstOrDefault(p => p.StudentId == x.StudentId)?.Level;
            });
            methodResult.Result = new PagingItemsModel<StudentInPlatformModel>(lists, request, totalItem);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
