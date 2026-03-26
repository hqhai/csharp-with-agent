// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Master.Application.Queries.ProgressMetrics
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Master.Application.Providers;
    using Fsel.Master.Domain.Entities;
    using Fsel.Master.Domain.IRepositories;
    using Fsel.Master.Domain.Models.EntityModels;
    using Fsel.Master.Domain.Models.Enums;
    using Fsel.Master.Domain.Models.QueryModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetProvinceLevelDistributionQuery : BaseProgressMetricsQueryModel, IRequest<MethodResult<ProvinceLevelResponse>>
    {
        public EnumUnitLevelType UnitLevel { get; set; }
    }

    public class GetProvinceLevelDistributionQueryHandler : IRequestHandler<GetProvinceLevelDistributionQuery, MethodResult<ProvinceLevelResponse>>
    {
        private readonly IMasterBaseRepository<StudentProfileReport> _studentRepository;
        private readonly IMasterBaseRepository<PlacementTestGroup> _placementTestGroupRepository;
        private readonly IMasterBaseRepository<StudentCompetitionEvent> _studentCompetitionEventRepository;
        private readonly IMasterBaseRepository<CompetitionEvent> _competitionEventRepository;
        private readonly IMasterBaseRepository<Program> _programRepository;
        private readonly IMasterBaseRepository<Subject> _subjectRepository;
        private readonly IMasterBaseRepository<Level> _levelRepository;
        private readonly IProgressMetricsQueryProvider _progressMetricsQueryProvider;

        public GetProvinceLevelDistributionQueryHandler(IMasterBaseRepository<StudentProfileReport> studentRepository, IMasterBaseRepository<PlacementTestGroup> placementTestGroupRepository, IMasterBaseRepository<StudentCompetitionEvent> studentCompetitionEventRepository, IMasterBaseRepository<CompetitionEvent> competitionEventRepository, IMasterBaseRepository<Program> programRepository, IMasterBaseRepository<Subject> subjectRepository, IMasterBaseRepository<Level> levelRepository, IProgressMetricsQueryProvider progressMetricsQueryProvider)
        {
            _studentRepository = studentRepository;
            _placementTestGroupRepository = placementTestGroupRepository;
            _studentCompetitionEventRepository = studentCompetitionEventRepository;
            _competitionEventRepository = competitionEventRepository;
            _programRepository = programRepository;
            _subjectRepository = subjectRepository;
            _levelRepository = levelRepository;
            _progressMetricsQueryProvider = progressMetricsQueryProvider;
        }

        public async Task<MethodResult<ProvinceLevelResponse>> Handle(GetProvinceLevelDistributionQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var methodResult = new MethodResult<ProvinceLevelResponse>();

            var baseQuery = _progressMetricsQueryProvider.JoinStudentWithCompetitionEvent();

            baseQuery = baseQuery.Where(x =>
                x.S.ProvinceId != null && x.S.ProvinceId != default
                && x.S.DistrictId != null && x.S.DistrictId != default
                && x.S.SchoolId != null && x.S.SchoolId != default);

            if (request.ProvinceIds?.Any() == true)
            {
                baseQuery = baseQuery.Where(x => x.S.ProvinceId.HasValue && request.ProvinceIds.Contains(x.S.ProvinceId.Value));
            }

            if (request.DistrictIds?.Any() == true)
            {
                baseQuery = baseQuery.Where(x => x.S.DistrictId.HasValue && request.DistrictIds.Contains(x.S.DistrictId.Value));
            }

            if (request.SchoolIds?.Any() == true)
            {
                baseQuery = baseQuery.Where(x => x.S.SchoolId.HasValue && request.SchoolIds.Contains(x.S.SchoolId.Value));
            }

            var unitQuery = baseQuery.Select(x => new
            {
                x.S.StudentId,
                UnitId = request.UnitLevel == EnumUnitLevelType.Province ? x.S.ProvinceId :
                         request.UnitLevel == EnumUnitLevelType.District ? x.S.DistrictId :
                         x.S.SchoolId,
                UnitName = request.UnitLevel == EnumUnitLevelType.Province ? x.S.ProvinceName :
                           request.UnitLevel == EnumUnitLevelType.District ? x.S.DistrictName :
                           x.S.SchoolName
            });

            var placementQuery = from u in unitQuery
                                 join p in _placementTestGroupRepository.Queryable
                                    on u.StudentId equals p.StudentId
                                 join prog in _programRepository.Queryable
                                    on p.ProgramId equals prog.ProgramId
                                 where p.Status == EnumResultStatus.Done
                                       && p.LevelId != null
                                       && prog.SubjectId == request.SubjectId
                                 select new
                                 {
                                     u.UnitId,
                                     u.UnitName,
                                     p.LevelId,
                                     p.ProgramId
                                 };

            if (request.ProgramIds != null && request.ProgramIds.Count > 0)
            {
                placementQuery = placementQuery.Where(p => p.ProgramId.HasValue && request.ProgramIds.Contains(p.ProgramId.Value));
            }

            if (request.LevelIds != null && request.LevelIds.Count > 0)
            {
                placementQuery = placementQuery.Where(p => p.LevelId.HasValue && request.LevelIds.Contains(p.LevelId.Value));
            }

            var rawData = await (from p in placementQuery
                                 join lvl in _levelRepository.Queryable
                                     on p.LevelId equals lvl.LevelId
                                 select new
                                 {
                                     p.UnitId,
                                     p.UnitName,
                                     lvl.LevelId,
                                     lvl.LevelName,
                                     lvl.LevelOrder
                                 }
                                 )
                                 .GroupBy(x => new
                                 {
                                     x.UnitId,
                                     x.UnitName,
                                     x.LevelId,
                                     x.LevelName,
                                     x.LevelOrder
                                 })
                                 .Select(g => new
                                 {
                                     g.Key.UnitId,
                                     g.Key.UnitName,
                                     g.Key.LevelId,
                                     g.Key.LevelName,
                                     g.Key.LevelOrder,
                                     Count = g.Count()
                                 })
                                 .ToListAsync(cancellationToken);

            var levelEntities = await _levelRepository.Queryable.ToListAsync(cancellationToken);
            var programEntities = await _programRepository.Queryable.ToListAsync(cancellationToken);

            var allLevels = rawData
                          .GroupBy(x => new { x.LevelId, x.LevelName, x.LevelOrder })
                          .Select(g => new
                          {
                              g.Key.LevelId,
                              g.Key.LevelName,
                              g.Key.LevelOrder
                          })
                          .OrderBy(x => x.LevelOrder)
                          .Select(x =>
                          {
                              var level = levelEntities.FirstOrDefault(p => p.LevelId == x.LevelId);
                              var program = programEntities.FirstOrDefault(p => p.ProgramId == level?.ProgramId);
                              return new ProvinceLevelModels
                              {
                                  LevelId = x.LevelId,
                                  LevelName = x.LevelName,
                                  DisplayOrder = level?.LevelOrder,
                                  ProgramName = program?.ProgramName,
                                  CountStudent = 0
                              };
                          })
                          .ToList();

            var result = rawData
                         .GroupBy(x => new { x.UnitId, x.UnitName })
                         .Select(g =>
                         {
                             var levels = allLevels
                                 .Select(l => new ProvinceLevelModels
                                 {
                                     LevelId = l.LevelId,
                                     LevelName = l.LevelName,
                                     CountStudent = 0
                                 })
                                 .ToList();

                             var dict = levels.ToDictionary(x => x.LevelId);

                             foreach (var item in g)
                             {
                                 if (dict.TryGetValue(item.LevelId, out var level))
                                 {
                                     level.CountStudent = item.Count;
                                 }
                             }

                             return new ProvinceLevelsModel
                             {
                                 UnitId = g.Key.UnitId ?? Guid.Empty,
                                 UnitName = g.Key.UnitName,
                                 Levels = levels
                             };
                         })
                         .ToList();

            methodResult.Result = new ProvinceLevelResponse
            {
                Data = result,
                AllLevels = allLevels.OrderBy(p => p.ProgramName).ThenBy(p => p.DisplayOrder).ToList()
            };

            return methodResult;
        }
    }
}
