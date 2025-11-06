// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Infrastructure.Repositories
{
    using Fsel.Core.Base;
    using Fsel.Identity.Domain.Entities;
    using Fsel.Identity.Domain.IRepositories;
    using Microsoft.EntityFrameworkCore;

    public class CompetitionEventsRepository : BaseRepository<CompetitionEvent>, ICompetitionEventsRepository
    {
        public CompetitionEventsRepository(UserDbContext dbContext, AuthContext authContext, AutoMapper.IMapper mapper) : base(dbContext, authContext, mapper)
        {
        }

        public async Task<string> GetEventCodeAsync(Guid? schoolId)
        {
            if (!schoolId.HasValue)
            {
                return string.Empty;
            }
            var competitionEvent = await Queryable.Where(x => x.SchoolIdsStr != null && x.SchoolIdsStr.Contains(schoolId.Value.ToString()))
                                            .OrderByDescending(x => x.CreatedDate)
                                            .FirstOrDefaultAsync();
            return competitionEvent?.EventCode ?? string.Empty;
        }
    }
}
