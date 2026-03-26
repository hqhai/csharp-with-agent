// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Master.Application.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Fsel.Common.Caching;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Master.Domain.Entities;
    using Fsel.Master.Domain.IRepositories;
    using Fsel.Shared.Constants;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    public interface IProgressMetricsQueryProvider
    {
        Task<IQueryable<StudentEventJoinModel>> QueryableStudentWithEventAsync();
    }

    public class ProgressMetricsQueryProvider : IProgressMetricsQueryProvider
    {
        private readonly IMasterBaseRepository<StudentProfileReport> _studentRepository;
        private readonly IMasterBaseRepository<StudentCompetitionEvent> _studentCompetitionEventRepository;
        private readonly ICompetitionEventRepository _competitionEventRepository;
        private readonly AuthContext _authContext;
        private readonly ICacheService<List<Guid>> _cacheService;
        private readonly ILogger<ProgressMetricsQueryProvider> _logger;

        public ProgressMetricsQueryProvider(
            IMasterBaseRepository<StudentProfileReport> studentRepository,
            IMasterBaseRepository<StudentCompetitionEvent> studentCompetitionEventRepository,
            ICompetitionEventRepository competitionEventRepository,
            AuthContext authContext,
            ICacheService<List<Guid>> cacheService,
            ILogger<ProgressMetricsQueryProvider> logger)
        {
            _studentRepository = studentRepository;
            _studentCompetitionEventRepository = studentCompetitionEventRepository;
            _competitionEventRepository = competitionEventRepository;
            _authContext = authContext;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<IQueryable<StudentEventJoinModel>> QueryableStudentWithEventAsync()
        {
            var eventCode = _authContext.ClaimsPrincipal?.FindFirstValue(JwtClaimConstant.EventCode);
            var schoolIds = _authContext.ClaimsPrincipal?.FindAll(JwtClaimConstant.SchoolIds).Select(c => c.Value.Parse<Guid>()).ToList();

            var studentQuery = _studentRepository.Queryable;
            if (schoolIds != null && schoolIds.Count > 0)
            {
                studentQuery = studentQuery.WhereBulkContains(schoolIds, x => x.SchoolId);
            }

            var eventQuery = _competitionEventRepository.Queryable;
            if (!string.IsNullOrEmpty(eventCode))
            {
                var cacheKey = $"ProgressMetrics:LeafEvents:{eventCode}".ToLower(CultureInfo.InvariantCulture);
                var cachedLeafIds = await _cacheService.GetAsync(cacheKey, TimeSpan.FromSeconds(CacheSettings.TimeCache.OneHour), async () =>
                {
                    var leafEvents = await _competitionEventRepository.GetLeafEventsByEventCodeAsync(eventCode);
                    return leafEvents.Select(x => x.CompetitionEventId).ToList();
                }, _logger) ?? new List<Guid>();

                if (cachedLeafIds.Count == 0)
                {
                    return Enumerable.Empty<StudentEventJoinModel>().AsQueryable();
                }

                eventQuery = eventQuery.WhereBulkContains(cachedLeafIds, x => x.CompetitionEventId);
            }

            return from s in studentQuery
                   join sce in _studentCompetitionEventRepository.Queryable
                       on s.StudentId equals sce.StudentId
                   join ce in eventQuery
                       on sce.CompetitionEventId equals ce.CompetitionEventId
                   select new StudentEventJoinModel
                   {
                       S = s,
                       Sce = sce,
                       Ce = ce
                   };
        }
    }

    public class StudentEventJoinModel
    {
        public StudentProfileReport S { get; set; } = null!;
        public StudentCompetitionEvent Sce { get; set; } = null!;
        public CompetitionEvent Ce { get; set; } = null!;
    }
}
