// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Queries.StudentQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Domain.Models.EntityModels;
    using Fsel.Identity.Domain.Models.QueryModels.Students;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetStudentsInPlatformQuery : GetStudentInPlatformQueryModel, IRequest<MethodResult<IList<StudentInPlatformModel>>>
    {
    }

    public class GetStudentsInPlatformQueryHandler : IRequestHandler<GetStudentsInPlatformQuery, MethodResult<IList<StudentInPlatformModel>>>
    {
        private readonly IPlatformRepository _platformRepository;
        private readonly IUserPlatformRepository _userPlatformRepository;
        private readonly UserManager<User> _userManager;
        private readonly IStudentRepository _studentRepository;
        private readonly RoleManager<Role> _roleManager;
        private readonly IUserRoleRepository _userRoleRepository;

        public GetStudentsInPlatformQueryHandler(IPlatformRepository platformRepository, IUserPlatformRepository userPlatformRepository, UserManager<User> userManager, IStudentRepository studentRepository, RoleManager<Role> roleManager, IUserRoleRepository userRoleRepository)
        {
            _platformRepository = platformRepository;
            _userPlatformRepository = userPlatformRepository;
            _userManager = userManager;
            _studentRepository = studentRepository;
            _roleManager = roleManager;
            _userRoleRepository = userRoleRepository;
        }

        public async Task<MethodResult<IList<StudentInPlatformModel>>> Handle(GetStudentsInPlatformQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<StudentInPlatformModel>>();

            Guid? platformId = null;
            if (request.PlatformCode.HasValue)
            {
                var platform = await _platformRepository.Queryable.FirstOrDefaultAsync(p => p.Code == request.PlatformCode, cancellationToken);
                platformId = platform?.Id;
            }
            var userRoleQuery = _userRoleRepository.GetQuery();
            var query = from a in _userPlatformRepository.Queryable.Where(n => !platformId.HasValue || n.PlatformId == platformId)
                        join b in _userManager.Users on a.UserId equals b.Id
                        join d in _studentRepository.Queryable on b.Id equals d.UserId
                        join ur in userRoleQuery on b.Id equals ur.UserId
                        join r in _roleManager.Roles on ur.RoleId equals r.Id
                        select new { b, r, d, a };
            query = query.Where(p => p.a.Status == request.Status);
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                var userNameQuery = query.Where(p => p.b.UserName!.Contains(request.Keyword));
                var codeQuery = query.Where(p => p.b.Code!.Contains(request.Keyword));
                query = userNameQuery.Union(codeQuery);
            }
            if (request.Role.HasValue)
            {
                query = query.Where(p => p.r.Name == request.Role.ToString());
            }

            var dataQuery = query.Select(i => new StudentInPlatformModel
            {
                Id = i.b.Id,
                Code = i.b.Code,
                UserName = i.b.UserName,
                Role = i.r.Name,
                StudentId = i.d.Id,
                UserPlatformStatus = i.a.Status,
                CreatedDate = i.a.CreatedDate,
            });
            methodResult.Result = await dataQuery.ToListAsync(cancellationToken);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
