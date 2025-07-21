namespace Fsel.Identity.Infrastructure.Repositories
{
    using System;
    using AutoMapper;
    using Fsel.Core.Base;
    using Fsel.Core.Entities;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;

    public class StudentEditHistoryRepository : BaseRepository<StudentEditHistory>, IStudentEditHistoryRepository
    {
        public StudentEditHistoryRepository(UserDbContext dbContext, AuthContext authContext, AutoMapper.IMapper mapper) : base(dbContext, authContext, mapper)
        {
        }
    }
}
