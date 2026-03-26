// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Master.Application.Queries.ProgressMetrics
{
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Master.Domain.Entities;
    using Fsel.Master.Domain.IRepositories;
    using Fsel.Master.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetPlacementTestDetailQuery : IRequest<MethodResult<PlacementTestDetailModel>>
    {
        public Guid TestGroupResultId { get; set; }
    }

    public class GetPlacementTestDetailQueryHandler : IRequestHandler<GetPlacementTestDetailQuery, MethodResult<PlacementTestDetailModel>>
    {
        private readonly IMasterBaseRepository<Program> _programRepository;
        private readonly IMasterBaseRepository<Subject> _subjectRepository;
        private readonly IMasterBaseRepository<Level> _levelRepository;
        private readonly IMasterBaseRepository<PlacementTestGroup> _placementTestGroupRepository;
        private readonly IMasterBaseRepository<PlacementTest> _placementTestRepository;
        private readonly IMasterBaseRepository<Skill> _skillRepository;

        public GetPlacementTestDetailQueryHandler(IMasterBaseRepository<Program> programRepository, IMasterBaseRepository<Subject> subjectRepository, IMasterBaseRepository<Level> levelRepository, IMasterBaseRepository<PlacementTestGroup> placementTestGroupRepository, IMasterBaseRepository<PlacementTest> placementTestRepository, IMasterBaseRepository<Skill> skillRepository)
        {
            _programRepository = programRepository;
            _subjectRepository = subjectRepository;
            _levelRepository = levelRepository;
            _placementTestGroupRepository = placementTestGroupRepository;
            _placementTestRepository = placementTestRepository;
            _skillRepository = skillRepository;
        }

        public async Task<MethodResult<PlacementTestDetailModel>> Handle(GetPlacementTestDetailQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<PlacementTestDetailModel>();

            var placementTestGroup = await (from pg in _placementTestGroupRepository.Queryable
                                            join l in _levelRepository.Queryable on pg.LevelId equals l.LevelId
                                            join p in _programRepository.Queryable on pg.ProgramId equals p.ProgramId
                                            join s in _subjectRepository.Queryable on p.SubjectId equals s.SubjectId
                                            where pg.TestGroupResultId == request.TestGroupResultId
                                            select new PlacementTestDetailModel
                                            {
                                                LevelId = l.LevelId,
                                                ProgramId = p.ProgramId,
                                                SubjectId = s.SubjectId,
                                                LevelName = l.LevelName,
                                                ProgramName = p.ProgramName,
                                                SubjectName = s.SubjectName,
                                            }).FirstOrDefaultAsync(cancellationToken);

            if (placementTestGroup == null)
            {
                return methodResult;
            }

            var skills = await _skillRepository.Queryable.ToListAsync(cancellationToken);

            var placementTests = await _placementTestRepository.Queryable.Where(p => p.TestGroupResultId == request.TestGroupResultId).OrderBy(p => p.CreatedDate).ToListAsync(cancellationToken);

            placementTestGroup.Modules = placementTests.Select(p =>
            new PlacementTestModuleModel()
            {
                SkillScores = p.SkillScores?.Select(x =>
                {
                    var skill = skills.FirstOrDefault(s => s.SkillId == x.SkillId);
                    return new SkillScore()
                    {
                        SkillId = skill?.SkillId,
                        SkillName = skill?.SkillName,
                        Scores = x.Scores,
                        Percent = x.Percent,
                        CorrectCount = x.CorrectCount,
                        CorrectQuestion = x.CorrectQuestion,
                        CountQuestion = x.CountQuestion,
                        SkillFilePath = x.SkillFilePath,
                        TotalCount = x.TotalCount,
                        TotalQuestion = x.TotalQuestion,
                    };
                }).OrderBy(p => p.SkillName).ToList(),
            }).ToList();

            methodResult.Result = placementTestGroup;

            return methodResult;
        }
    }
}
