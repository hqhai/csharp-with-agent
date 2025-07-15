using Fsel.Core.Base;
using Fsel.Identity.Domain.Entities;
using Fsel.Identity.Domain.IRepositories;

namespace Fsel.Identity.Infrastructure.Repositories
{
    public class PermissionRepository : BaseRepository<Permission>, IPermissionRepository
    {
        public PermissionRepository(UserDbContext dbContext, AuthContext authContext, AutoMapper.IMapper mapper) : base(dbContext, authContext, mapper)
        {
        }
    }
}
