// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Infrastructure.Repositories
{
    using AutoMapper;
    using Fsel.Core.Base;
    using Fsel.System.Domain.Entities;
    using Fsel.System.Domain.IRepositories;

    public class StudentTechieRepository : BaseRepository<StudentTechie>, IStudentTechieRepository
    {
        public StudentTechieRepository(SystemDbContext dbContext, SystemReadDbContext systemReadDb, AuthContext authContext, IMapper mapper)
            : base(dbContext, systemReadDb, authContext, mapper)
        {
        }
    }
}
