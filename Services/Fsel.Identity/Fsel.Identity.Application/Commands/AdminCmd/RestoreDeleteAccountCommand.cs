namespace Fsel.Identity.Application.Commands.AdminCmd
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base.Managers;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Fsel.Identity.Infrastructure;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class RestoreDeleteAccountCommand : IRequest<MethodResult<bool>>
    {
        public IList<Guid>? UserIds { get; set; }
    }

    public class RestoreDeleteAccountCommandHandler : IRequestHandler<RestoreDeleteAccountCommand, MethodResult<bool>>
    {
        private readonly IStudentRepository _studentRepository;
        private readonly UserDbContext _userDbContext;
        private readonly UserManager<User> _userManager;

        public RestoreDeleteAccountCommandHandler(IStudentRepository studentRepository, UserDbContext userDbContext, UserManager<User> userManager)
        {
            _studentRepository = studentRepository;
            _userDbContext = userDbContext;
            _userManager = userManager;
        }

        public async Task<MethodResult<bool>> Handle(RestoreDeleteAccountCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<bool>();

            if (request.UserIds == null || !request.UserIds.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.Required));
                return methodResult;
            }

            var query = await (from u in _userDbContext.Users.IgnoreQueryFilters()

                               join s in _studentRepository.Queryable.IgnoreQueryFilters()
                               on u.Id equals s.UserId

                               where request.UserIds.Contains(u.Id)

                               select new { Users = u, Students = s }).ToListAsync(cancellationToken);

            var users = query.Select(p => p.Users).ToList();
            if (users == null || !users.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(query));
                return methodResult;
            }

            var userNames = users.Where(p => !string.IsNullOrEmpty(p.UserName)).Select(p => p.UserName);
            if (userNames == null || !userNames.Any())
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(userNames));
                return methodResult;
            }

            var isUserAlreadyExist = await _userManager.Users.AnyAsync(p => userNames.Contains(p.UserName), cancellationToken);
            if (isUserAlreadyExist)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(userNames));
                return methodResult;
            }

            var students = query.Select(p => p.Students).ToList();

            bool hasDuplicateStudent = students.GroupBy(p => p.UserId)
                         .Any(g => g.Count() > 1);

            if (hasDuplicateStudent)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataAlreadyExist), nameof(hasDuplicateStudent));
                return methodResult;
            }

            await _studentRepository.ExecuteTransactionAsync(async () =>
            {
                if (users.Any())
                {
                    users.ForEach(p => p.IsDeleted = false);
                    _userDbContext.Users.UpdateRange(users);
                }
                if (students.Any())
                {
                    students.ForEach(p => p.IsDeleted = false);
                    _studentRepository.UpdateList(students);
                }

                await _studentRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                methodResult.Result = true;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            });

            return methodResult;
        }
    }
}
