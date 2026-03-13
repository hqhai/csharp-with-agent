// Copyright (c) Atlantic. All rights reserved.

using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Common.Enums;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.Course.Domain.IRepositories;
using Fsel.Course.Domain.Models.EntityModels;
using Fsel.Course.Domain.Models.EntityModels.V1i1;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using LessonSearchModel = Fsel.Course.Domain.Models.EntityModels.V1i1.LessonSearchModel;

namespace Fsel.Course.Application.Queries.UnitQuery
{
    public class GetUnitQuery : IRequest<MethodResult<UnitModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetUnitQueryHandler : IRequestHandler<GetUnitQuery, MethodResult<UnitModel>>
    {
        private readonly IUnitRepository _unitRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ITestRepository _testRepository;
        private readonly ITestSectionRepository _testSectionRepository;
        private readonly IMapper _mapper;

        public GetUnitQueryHandler(IMapper mapper,
            IUnitRepository unitRepository,
            ILessonRepository lessonRepository,
            ICategoryRepository categoryRepository,
            ITestRepository testRepository,
            ITestSectionRepository testSectionRepository)
        {
            _unitRepository = unitRepository;
            _lessonRepository = lessonRepository;
            _categoryRepository = categoryRepository;
            _testRepository = testRepository;
            _testSectionRepository = testSectionRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<UnitModel>> Handle(GetUnitQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            MethodResult<UnitModel> methodResult = new MethodResult<UnitModel>();

            var unit = await _unitRepository.ReadQueryable.Where(x => x.Id == request.Id)
                .Include(x => x.UnitModules)
                .FirstOrDefaultAsync(cancellationToken: cancellationToken);

            if (unit == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), "unit");
                return methodResult;
            }

            var unitModel = _mapper.Map<UnitModel>(unit);
            var originals = unit.UnitModules.Select(x => x.OriginalId).ToList();
            if (originals.Any())
            {
                var lessons = await _lessonRepository.ReadQueryable
                .Where(x => originals.Contains(x.OriginalId) && x.VersionStatus == EnumVersionStatus.LastVersion && !x.IsArchive)
                .Select(x => new LessonSearchModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    LevelId = x.LevelId,
                    ProgramId = x.ProgramId,
                    NameLevel = x.Level != null ? x.Level.Name : string.Empty,
                    NameProgram = x.Category != null ? x.Category.Name : string.Empty,
                    Overview = x.InstructionContent,
                    OriginalId = x.OriginalId,
                    Skills = x.LessonInstructions.Select(i => new SkillDTO
                    {
                        Name = i.Skill.Name ?? string.Empty,
                        FilePath = i.Skill.FilePath ?? string.Empty
                    }).ToList()
                })
                .ToListAsync(cancellationToken: cancellationToken);

                var tests = await _testRepository.ReadQueryable
               .Where(x => originals.Contains(x.OriginalId) && x.VersionStatus == EnumVersionStatus.LastVersion && !x.IsArchive)
               .Select(x => new PlacementTestSearchModel
               {
                   Id = x.Id,
                   Name = x.Name,
                   LevelId = x.LevelId,
                   ProgramId = x.ProgramId,
                   LevelCode = x.Level != null ? x.Level.Code : string.Empty,
                   LevelName = x.Level != null ? x.Level.Name : string.Empty,
                   NameProgram = x.Program != null ? x.Program.Name : string.Empty,
                   OriginalId = x.OriginalId,
                   Skills = _testSectionRepository.ReadQueryable.Include(x => x.Skill)
                   .Where(y => y.TestId == x.Id && y.ParentId == null)
                   .Select(i => new SkillDTO
                   {
                       Name = i.Skill.Name ?? string.Empty,
                       FilePath = i.Skill.FilePath ?? string.Empty
                   }).ToList()
               })
               .ToListAsync(cancellationToken: cancellationToken);

                unitModel.UnitModules?.ForEach(x =>
                {
                    x.Lesson = lessons.FirstOrDefault(l => l.OriginalId == x.OriginalId);
                    x.Test = tests.FirstOrDefault(l => l.OriginalId == x.OriginalId);
                });
            }

            var project = await _categoryRepository.GetSecondLevelFromRootAsync(unitModel.ProgramId, cancellationToken);
            unitModel.ProjectId = project?.Id;

            methodResult.Result = unitModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
