// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Repositories
{
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Microsoft.EntityFrameworkCore;
    using AutoMapper;

    public class HomeWorkQuestionRepository : BaseRepository<HomeWorkQuestion>, IHomeWorkQuestionRepository
    {
        public HomeWorkQuestionRepository(CourseDbContext dbContext, CourseReadDbContext readDbContext, AuthContext authContext, IMapper mapper)
            : base(dbContext, readDbContext, authContext, mapper)
        {
        }

        public async Task<bool> GetCheckByIdsAsync(IEnumerable<Guid>? ids)
        {
            try
            {
                return await Queryable.AnyAsync(e => ids != null && ids.Contains(e.Id));
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
