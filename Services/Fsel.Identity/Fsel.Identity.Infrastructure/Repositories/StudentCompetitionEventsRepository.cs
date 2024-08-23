// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Repositories
{
    using Fsel.Core.Base;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;

    public class StudentCompetitionEventRepository : BaseRepository<StudentCompetitionEvent>, IStudentCompetitionEventsRepository
    {
        public StudentCompetitionEventRepository(UserDbContext dbContext, AuthContext authContext, AutoMapper.IMapper mapper) : base(dbContext, authContext, mapper)
        {
        }
    }
}
