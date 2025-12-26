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

    public class AiFeatureConfigRepository  : BaseRepository<AICriteriaConfigs>, IAiCriteriaConfigRepository
    {
        private readonly CourseDbContext _dbContext;

        public AiFeatureConfigRepository(CourseDbContext dbContext, CourseReadDbContext readDbContext, AuthContext authContext, IMapper mapper) : base(dbContext, readDbContext, authContext, mapper)
        {
            _dbContext = dbContext;
        }

        public async Task<List<AICriteriaConfigs>> GetAllByAiPromptManagerIdAsync(Guid aiPromptManagerId)
        {
            return await _dbContext.AICriteriaConfigs
                .Where(x => x.AiPromptManagerId == aiPromptManagerId && x.VersionStatus == EnumVersionStatus.LastVersion)
                .ToListAsync();
        }

        public async Task<AICriteriaConfigs?> GetLastVersionByOriginalIdAsync(Guid originalId)
        {
            return await _dbContext.AICriteriaConfigs
                .Where(x => x.OriginalId == originalId && x.VersionStatus == EnumVersionStatus.LastVersion)
                .FirstOrDefaultAsync();
        }
    }
}
