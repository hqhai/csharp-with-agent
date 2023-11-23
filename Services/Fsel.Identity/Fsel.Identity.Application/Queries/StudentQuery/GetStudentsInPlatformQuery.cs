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
        private readonly IHumanRepository _humanRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly RoleManager<Role> _roleManager;
        private readonly IUserRoleRepository _userRoleRepository;

        public GetStudentsInPlatformQueryHandler(IPlatformRepository platformRepository, IUserPlatformRepository userPlatformRepository, UserManager<User> userManager, IHumanRepository humanRepository, IStudentRepository studentRepository, RoleManager<Role> roleManager, IUserRoleRepository userRoleRepository)
        {
            _platformRepository = platformRepository;
            _userPlatformRepository = userPlatformRepository;
            _userManager = userManager;
            _humanRepository = humanRepository;
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
                        join c in _humanRepository.Queryable on b.Id equals c.UserId
                        join d in _studentRepository.Queryable on c.Id equals d.HumanId
                        join ur in userRoleQuery on b.Id equals ur.UserId
                        join r in _roleManager.Roles on ur.RoleId equals r.Id
                        select new StudentInPlatformModel
                        {
                            Id = b.Id,
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
            if (request.Role.HasValue)
            {
                query = query.Where(p => p.Role == request.Role.ToString());
            }
            methodResult.Result = query.ToList();
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
