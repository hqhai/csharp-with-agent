// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Master.Domain.IRepositories
{
    using Fsel.Master.Domain.Entities;

    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public interface ICompetitionEventRepository : IMasterBaseRepository<CompetitionEvent>
    {
        /// <summary>
        /// Lấy tất cả leaf CompetitionEvent (con nhất) thuộc cây của eventCode bằng đệ quy trên memory (LINQ).
        /// </summary>
        Task<List<CompetitionEvent>> GetLeafEventsByEventCodeAsync(string? eventCode, CancellationToken cancellationToken = default);
    }
}
