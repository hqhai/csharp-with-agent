// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Master.Infrastructure.Repositories
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Master.Domain.Entities;
    using Fsel.Master.Domain.IRepositories;
    using Microsoft.EntityFrameworkCore;

    public class CompetitionEventRepository : MasterBaseRepository<CompetitionEvent>, ICompetitionEventRepository
    {
        public CompetitionEventRepository(MasterDBContext context) : base(context)
        {
        }

        public async Task<List<CompetitionEvent>> GetLeafEventsByEventCodeAsync(string? eventCode, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(eventCode))
            {
                return new List<CompetitionEvent>();
            }

            var rootEvents = await Queryable.Where(e => e.EventCode == eventCode).ToListAsync(cancellationToken);
            if (rootEvents.Count == 0)
            {
                return new List<CompetitionEvent>();
            }

            var allFetchedEvents = new List<CompetitionEvent>(rootEvents);
            var currentLevelParentIds = rootEvents.Select(e => (Guid?)e.CompetitionEventId).ToList();

            while (currentLevelParentIds.Count > 0)
            {
                var children = await Queryable
                    .Where(e => currentLevelParentIds.Contains(e.ParentEventId))
                    .ToListAsync(cancellationToken);

                if (children.Count == 0)
                {
                    break;
                }

                allFetchedEvents.AddRange(children);
                currentLevelParentIds = children.Select(e => (Guid?)e.CompetitionEventId).ToList();
            }

            var allParentIds = allFetchedEvents.Select(e => e.ParentEventId).Distinct().ToList();
            var leafNodes = allFetchedEvents.Where(e => !allParentIds.Contains(e.CompetitionEventId)).ToList();
            return leafNodes;
        }
    }
}
