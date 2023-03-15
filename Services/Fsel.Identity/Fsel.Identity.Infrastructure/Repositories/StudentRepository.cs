// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Repositories
{
    using Fsel.Core.Base;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;

    public class StudentRepository : BaseIdentityRepository<Student, User>, IStudentRepository
    {
        public StudentRepository(UserDbContext dbContext, AuthContext authContext) : base(dbContext, authContext)
        {
        }
    }
}
