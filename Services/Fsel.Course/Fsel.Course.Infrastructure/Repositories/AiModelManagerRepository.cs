// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Repositories
{
    using System;
    using System.Threading.Tasks;
    using AutoMapper;
    using Core.Base;
    using Domain.Entities;
    using Domain.IRepositories;
    using Microsoft.EntityFrameworkCore;

    public class AiModelManagerRepository : BaseRepository<AiModelManager>, IAiModelManagerRepository
    {
        public AiModelManagerRepository(CourseDbContext dbContext, CourseReadDbContext readDbContext, AuthContext authContext, IMapper mapper) : base(dbContext, readDbContext, authContext, mapper)
        {
        }

        public async Task<bool> IsFeature(Guid id)
        {
            return await Queryable
                .Include(x => x.AiModelFeatures.Where(f => !f.IsDeleted))
                .AllAsync(x => x.Id == id && x.AiModelFeatures.Count > 0);
        }
    }
}
