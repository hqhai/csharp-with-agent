// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Master.Application.Queries.FilterQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Master.Domain.Entities;
    using Fsel.Master.Domain.IRepositories;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class SubjectFilterItem
    {
        public Guid Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }

        public IList<SubjectFilterItem>? Children { get; set; }
    }

    public class GetSubjectsQuery : IRequest<MethodResult<IList<SubjectFilterItem>>>
    {
    }

    public class GetSubjectsQueryHandler : IRequestHandler<GetSubjectsQuery, MethodResult<IList<SubjectFilterItem>>>
    {
        private readonly IMasterBaseRepository<PlacementTestGroup> _placementTestRepository;
        private readonly IMasterBaseRepository<Subject> _subjectRepository;
        private readonly IMasterBaseRepository<Program> _programRepository;
        private readonly IMasterBaseRepository<Level> _levelRepository;

        public GetSubjectsQueryHandler(IMasterBaseRepository<PlacementTestGroup> placementTestRepository, IMasterBaseRepository<Subject> subjectRepository, IMasterBaseRepository<Program> programRepository, IMasterBaseRepository<Level> levelRepository)
        {
            _placementTestRepository = placementTestRepository;
            _subjectRepository = subjectRepository;
            _programRepository = programRepository;
            _levelRepository = levelRepository;
        }

        public async Task<MethodResult<IList<SubjectFilterItem>>> Handle(GetSubjectsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<SubjectFilterItem>>();

            var allSubjects = await _subjectRepository.Queryable.AsNoTracking()
                .Select(s => new { s.SubjectId, s.SubjectCode, s.SubjectName })
                .ToListAsync(cancellationToken);

            var allPrograms = await _programRepository.Queryable.AsNoTracking()
                .Select(p => new { p.ProgramId, p.ProgramCode, p.ProgramName, p.SubjectId })
                .ToListAsync(cancellationToken);

            var allLevels = await _levelRepository.Queryable.AsNoTracking()
                .Select(l => new { l.LevelId, l.LevelCode, l.LevelName, l.ProgramId, l.LevelOrder })
                .ToListAsync(cancellationToken);

            var levelsByProgram = allLevels
                .GroupBy(l => l.ProgramId)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderBy(lv => lv.LevelOrder)
                          .Select(lv => new SubjectFilterItem
                          {
                              Id = lv.LevelId,
                              Code = lv.LevelCode,
                              Name = lv.LevelName
                          }).ToList()
                );

            var programsBySubject = allPrograms
                .GroupBy(p => p.SubjectId)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderBy(p => p.ProgramCode)
                          .Select(p => new SubjectFilterItem
                          {
                              Id = p.ProgramId,
                              Code = p.ProgramCode,
                              Name = p.ProgramName,
                              Children = levelsByProgram.GetValueOrDefault(p.ProgramId)
                          }).ToList()
                );

            var result = allSubjects
                .OrderByDescending(s => s.SubjectName == "SubjectENG")
                .ThenBy(s => s.SubjectName)
                .Select(s => new SubjectFilterItem
                {
                    Id = s.SubjectId,
                    Code = s.SubjectCode,
                    Name = s.SubjectName,
                    Children = programsBySubject.GetValueOrDefault(s.SubjectId)
                }).ToList();

            methodResult.Result = result;
            return methodResult;
        }
    }
}
