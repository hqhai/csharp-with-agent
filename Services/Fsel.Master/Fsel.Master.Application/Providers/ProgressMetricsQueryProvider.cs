// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Master.Application.Providers
{
    using System;
    using System.Linq;
    using System.Security.Claims;
    using Fsel.Common.Helpers;
    using Fsel.Core.Base;
    using Fsel.Master.Domain.Entities;
    using Fsel.Master.Domain.IRepositories;
    using Fsel.Shared.Constants;

    public interface IProgressMetricsQueryProvider
    {
        IQueryable<StudentEventJoinModel> JoinStudentWithCompetitionEvent();
    }

    public class ProgressMetricsQueryProvider : IProgressMetricsQueryProvider
    {
        private readonly IMasterBaseRepository<StudentProfileReport> _studentRepository;
        private readonly IMasterBaseRepository<StudentCompetitionEvent> _studentCompetitionEventRepository;
        private readonly IMasterBaseRepository<CompetitionEvent> _competitionEventRepository;
        private readonly AuthContext _authContext;

        public ProgressMetricsQueryProvider(
            IMasterBaseRepository<StudentProfileReport> studentRepository,
            IMasterBaseRepository<StudentCompetitionEvent> studentCompetitionEventRepository,
            IMasterBaseRepository<CompetitionEvent> competitionEventRepository,
            AuthContext authContext)
        {
            _studentRepository = studentRepository;
            _studentCompetitionEventRepository = studentCompetitionEventRepository;
            _competitionEventRepository = competitionEventRepository;
            _authContext = authContext;
        }

        public IQueryable<StudentEventJoinModel> JoinStudentWithCompetitionEvent()
        {
            var eventCode = _authContext.ClaimsPrincipal?.FindFirstValue(JwtClaimConstant.EventCode);
            var schoolIds = _authContext.ClaimsPrincipal?.FindAll(JwtClaimConstant.SchoolIds).Select(c => c.Value.Parse<Guid>()).ToList();

            return from s in _studentRepository.Queryable
                   join sce in _studentCompetitionEventRepository.Queryable
                       on s.StudentId equals sce.StudentId
                   join ce in _competitionEventRepository.Queryable
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
