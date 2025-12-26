// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Infrastructure.Repositories
{
    using AutoMapper;
    using Core.Base;
    using Domain.Entities;
    using Domain.IRepositories;
    using Fsel.Common.Enums;
    using Microsoft.EntityFrameworkCore;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class AiPromptManagerRepository : BaseRepository<AiPromptManager>, IAiPromptManagerRepository
    {
        private readonly CourseDbContext _dbContext;

        public AiPromptManagerRepository(CourseDbContext dbContext, CourseReadDbContext readDbContext, AuthContext authContext, IMapper mapper) : base(dbContext, readDbContext, authContext, mapper)
        {
            _dbContext = dbContext;
        }

        public async Task<AiPromptManager?> GetLastVersionByOriginalIdAsync(Guid originalId)
        {
            return await _dbContext.AiPromptManagers
                .Where(x => x.OriginalId == originalId && x.VersionStatus == EnumVersionStatus.LastVersion)
                .FirstOrDefaultAsync();
        }

        public async Task<List<AiPromptManager>> GetAllVersionsByOriginalIdAsync(Guid originalId)
        {
            return await _dbContext.AiPromptManagers
                .Where(x => x.OriginalId == originalId || x.Id == originalId)
                .OrderByDescending(x => x.Version)
                .ToListAsync();
        }

        public async Task<List<AiPromptManager>> GetVersionsByProjectIdAsync(Guid projectId)
        {
            return await _dbContext.AiPromptManagers
                .Where(x => x.ProjectId == projectId && x.VersionStatus == EnumVersionStatus.LastVersion)
                .OrderByDescending(x => x.UpdatedDate)
                .ToListAsync();
        }

        public async Task MarkAsOldVersionAsync(Guid id)
        {
            var entity = await _dbContext.AiPromptManagers.FindAsync(id);
            if (entity != null)
            {
                entity.VersionStatus = EnumVersionStatus.OldVersion;
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<int> GetNextVersionNumberAsync(Guid originalId)
        {
            var lastVersion = await _dbContext.AiPromptManagers
                .Where(x => x.OriginalId == originalId || x.Id == originalId)
                .MaxAsync(x => (int?)x.Version) ?? 0;

            return lastVersion + 1;
        }
    }
}
