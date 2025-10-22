// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Domain.IRepositories
{
    using Fsel.Core.Base.Interfaces;
    using Fsel.Identity.Domain.Entities;

    public interface ICompetitionEventsRepository : IRepository<CompetitionEvent>
    {
        Task<string> GetEventCodeAsync(Guid? schoolId);
    }
}
