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

    public class GetProvinceLevelDistributionQuery : BaseProgressMetricsQueryModel, IRequest<MethodResult<ProvinceLevelResponse>>
    {
        public EnumUnitLevelType UnitLevel { get; set; }
    }

    public class GetProvinceLevelDistributionQueryHandler : IRequestHandler<GetProvinceLevelDistributionQuery, MethodResult<ProvinceLevelResponse>>
    {
        private readonly IMasterBaseRepository<StudentProfileReport> _studentRepository;
        private readonly IMasterBaseRepository<PlacementTestReport> _placementTestRepository;
        private readonly IMasterBaseRepository<StudentCompetitionEvent> _studentCompetitionEventRepository;
        private readonly IMasterBaseRepository<CompetitionEvent> _competitionEventRepository;
        private readonly IMasterBaseRepository<Program> _programRepository;
        private readonly IMasterBaseRepository<Subject> _subjectRepository;
        private readonly IMasterBaseRepository<Level> _levelRepository;

        public GetProvinceLevelDistributionQueryHandler(IMasterBaseRepository<StudentProfileReport> studentRepository, IMasterBaseRepository<PlacementTestReport> placementTestRepository, IMasterBaseRepository<StudentCompetitionEvent> studentCompetitionEventRepository, IMasterBaseRepository<CompetitionEvent> competitionEventRepository, IMasterBaseRepository<Program> programRepository, IMasterBaseRepository<Subject> subjectRepository, IMasterBaseRepository<Level> levelRepository)
        {
            _studentRepository = studentRepository;
            _placementTestRepository = placementTestRepository;
            _studentCompetitionEventRepository = studentCompetitionEventRepository;
            _competitionEventRepository = competitionEventRepository;
            _programRepository = programRepository;
            _subjectRepository = subjectRepository;
            _levelRepository = levelRepository;
        }

        public async Task<MethodResult<ProvinceLevelResponse>> Handle(GetProvinceLevelDistributionQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var methodResult = new MethodResult<ProvinceLevelResponse>();

            var baseQuery = from s in _studentRepository.Queryable
                            join sce in _studentCompetitionEventRepository.Queryable
                                on s.StudentId equals sce.StudentId
                            where s.ProvinceId != null
                                  && s.DistrictId != null
                                  && s.SchoolId != null
                                  && s.ProvinceId != default
                                  && s.DistrictId != default
                                  && s.SchoolId != default
                            select new
                            {
                                s.StudentId,
                                s.ProvinceId,
                                s.ProvinceName,
                                s.DistrictId,
                                s.DistrictName,
                                s.SchoolId,
                                s.SchoolName
                            };

            if (request.ProvinceIds?.Any() == true)
            {
                baseQuery = baseQuery.Where(x =>
                    x.ProvinceId.HasValue &&
                    request.ProvinceIds.Contains(x.ProvinceId.Value));
            }

            if (request.DistrictIds?.Any() == true)
            {
                baseQuery = baseQuery.Where(x =>
                    x.DistrictId.HasValue &&
                    request.DistrictIds.Contains(x.DistrictId.Value));
            }

            if (request.SchoolIds?.Any() == true)
            {
                baseQuery = baseQuery.Where(x =>
                    x.SchoolId.HasValue &&
                    request.SchoolIds.Contains(x.SchoolId.Value));
            }

            var unitQuery = baseQuery.Select(x => new
            {
                x.StudentId,

                UnitId =
            request.UnitLevel == EnumUnitLevelType.Province ? x.ProvinceId :
            request.UnitLevel == EnumUnitLevelType.District ? x.DistrictId :
            x.SchoolId,

                UnitName =
            request.UnitLevel == EnumUnitLevelType.Province ? x.ProvinceName :
            request.UnitLevel == EnumUnitLevelType.District ? x.DistrictName :
            x.SchoolName
            });

            var placementQuery = from u in unitQuery
                                 join p in _placementTestRepository.Queryable
                                    on u.StudentId equals p.StudentId
                                 where p.Status == EnumResultStatus.Done
                                       && p.LevelId != null
                                 select new
                                 {
                                     u.UnitId,
                                     u.UnitName,
                                     p.LevelId,
                                     p.ProgramId
                                 };

            if (request.SubjectIds?.Any() == true)
            {
                placementQuery =
                    from p in placementQuery
                    join prog in _programRepository.Queryable
                        on p.ProgramId equals prog.ProgramId
                    where request.SubjectIds.Contains(prog.SubjectId)
                    select p;
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

            var allLevels = rawData
                          .GroupBy(x => new { x.LevelId, x.LevelName, x.LevelOrder })
                          .Select(g => new
                          {
                              g.Key.LevelId,
                              g.Key.LevelName,
                              g.Key.LevelOrder
                          })
                          .OrderBy(x => x.LevelOrder)
                          .Select(x => new ProvinceLevelModels
                          {
                              LevelId = x.LevelId,
                              LevelName = x.LevelName,
                              CountStudent = 0
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
                AllLevels = allLevels
            };

            return methodResult;
        }
    }
}
