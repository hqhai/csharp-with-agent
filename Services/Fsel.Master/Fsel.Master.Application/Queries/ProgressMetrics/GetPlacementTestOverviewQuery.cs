// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Master.Application.Queries.ProgressMetrics
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Master.Domain.Entities;
    using Fsel.Master.Domain.IRepositories;
    using Fsel.Master.Domain.Models.EntityModels;
    using Fsel.Master.Domain.Models.Enums;
    using Fsel.Master.Domain.Models.QueryModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetPlacementTestOverviewQuery : BaseProgressMetricsQueryModel, IRequest<MethodResult<PlacementTestDashboardModel>>
    {
    }

    public class GetPlacementTestOverviewQueryHandler : IRequestHandler<GetPlacementTestOverviewQuery, MethodResult<PlacementTestDashboardModel>>
    {
        private readonly IMasterBaseRepository<StudentProfileReport> _studentRepository;
        private readonly IMasterBaseRepository<PlacementTestGroupReport> _placementTestRepository;
        private readonly IMasterBaseRepository<StudentCompetitionEvent> _studentCompetitionEventRepository;
        private readonly IMasterBaseRepository<CompetitionEvent> _competitionEventRepository;
        private readonly IMasterBaseRepository<Program> _programRepository;
        private readonly IMasterBaseRepository<Subject> _subjectRepository;
        private readonly IMasterBaseRepository<Level> _levelRepository;

        public GetPlacementTestOverviewQueryHandler(IMasterBaseRepository<StudentProfileReport> studentRepository, IMasterBaseRepository<PlacementTestGroupReport> placementTestRepository, IMasterBaseRepository<StudentCompetitionEvent> studentCompetitionEventRepository, IMasterBaseRepository<CompetitionEvent> competitionEventRepository, IMasterBaseRepository<Program> programRepository, IMasterBaseRepository<Subject> subjectRepository, IMasterBaseRepository<Level> levelRepository)
        {
            _studentRepository = studentRepository;
            _placementTestRepository = placementTestRepository;
            _studentCompetitionEventRepository = studentCompetitionEventRepository;
            _competitionEventRepository = competitionEventRepository;
            _programRepository = programRepository;
            _subjectRepository = subjectRepository;
            _levelRepository = levelRepository;
        }

        public async Task<MethodResult<PlacementTestDashboardModel>> Handle(GetPlacementTestOverviewQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PlacementTestDashboardModel>();

            var baseQuery = from s in _studentRepository.Queryable
                            join sce in _studentCompetitionEventRepository.Queryable
                                on s.StudentId equals sce.StudentId
                            where s.ProvinceId != null
                              && s.DistrictId != null
                              && s.SchoolId != null
                              && s.ProvinceId != default
                              && s.DistrictId != default
                              && s.SchoolId != default
                            select new StudentPlacementInfoModel
                            {
                                StudentId = s.StudentId,
                                CompetitionEventId = sce.CompetitionEventId,
                                ProvinceId = s.ProvinceId,
                                DistrictId = s.DistrictId,
                                SchoolId = s.SchoolId
                            };

            if (request.ProvinceIds != null && request.ProvinceIds.Count > 0)
            {
                baseQuery = baseQuery.Where(x => x.ProvinceId.HasValue && request.ProvinceIds.Contains(x.ProvinceId.Value));
            }

            if (request.DistrictIds != null && request.DistrictIds.Count > 0)
            {
                baseQuery = baseQuery.Where(x => x.DistrictId.HasValue && request.DistrictIds.Contains(x.DistrictId.Value));
            }

            if (request.SchoolIds != null && request.SchoolIds.Count > 0)
            {
                baseQuery = baseQuery.Where(x => x.SchoolId.HasValue && request.SchoolIds.Contains(x.SchoolId.Value));
            }

            var totalRegistered = await baseQuery.Select(x => x.StudentId).Distinct().CountAsync(cancellationToken);

            var placementQuery = from b in baseQuery
                                 join p in _placementTestRepository.Queryable
                                    on b.StudentId equals p.StudentId into gj
                                 from p in gj.DefaultIfEmpty()
                                 select new
                                 {
                                     b.StudentId,
                                     p.Status,
                                     p.LevelId,
                                     p.ProgramId
                                 };

            placementQuery = from p in placementQuery
                             join prog in _programRepository.Queryable on p.ProgramId equals prog.ProgramId
                             where prog.SubjectId == request.SubjectId
                             select p;

            if (request.ProgramIds != null && request.ProgramIds.Count > 0)
            {
                placementQuery = placementQuery.Where(p => p.ProgramId.HasValue && request.ProgramIds.Contains(p.ProgramId.Value));
            }

            if (request.LevelIds != null && request.LevelIds.Count > 0)
            {
                placementQuery = placementQuery.Where(p => p.LevelId.HasValue && request.LevelIds.Contains(p.LevelId.Value));
            }

            var totalStarted = await placementQuery.Where(x => x.Status == EnumResultStatus.Process || x.Status == EnumResultStatus.Done)
                                                   .Select(x => x.StudentId)
                                                   .Distinct()
                                                   .CountAsync(cancellationToken);

            var totalCompleted = await placementQuery.Where(x => x.Status == EnumResultStatus.Done && x.LevelId.HasValue)
                                                     .Select(x => x.StudentId)
                                                     .Distinct()
                                                     .CountAsync(cancellationToken);

            var levelData = await (from p in placementQuery
                                   join lvl in _levelRepository.Queryable
                                         on p.LevelId equals lvl.LevelId
                                   where p.Status == EnumResultStatus.Done &&
                                         p.LevelId.HasValue
                                   group lvl by new { lvl.LevelId, lvl.LevelName, lvl.LevelOrder, lvl.ProgramId } into g
                                   select new LevelDistributionModel
                                   {
                                       LevelId = g.Key.LevelId,
                                       LevelName = g.Key.LevelName,
                                       DisplayOrder = g.Key.LevelOrder,
                                       ProgramId = g.Key.ProgramId,
                                       StudentCount = g.Count()
                                   }
                                  ).OrderBy(p => p.DisplayOrder).ToListAsync(cancellationToken);

            var programs = await _programRepository.Queryable.ToListAsync(cancellationToken);

            foreach (var item in levelData)
            {
                item.Percentage = totalCompleted == 0 ? 0 : (double)item.StudentCount / totalCompleted * 100;
                item.ProgramName = programs.FirstOrDefault(p => p.ProgramId == item.ProgramId)?.ProgramName;
            }

            var dashboardModel = new PlacementTestDashboardModel
            {
                Summary = new PlacementTestSummaryModel
                {
                    TotalRegistered = totalRegistered,
                    TotalStarted = totalStarted,
                    TotalCompleted = totalCompleted
                },
                LevelDistributions = levelData.OrderBy(p => p.ProgramName).ThenBy(p => p.DisplayOrder).ToList()
            };

            methodResult.Result = dashboardModel;
            return methodResult;
        }
    }
}
