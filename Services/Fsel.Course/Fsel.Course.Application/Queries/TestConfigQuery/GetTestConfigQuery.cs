// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.TestConfigQuery
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.TestConfig;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels.TestConfig;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetTestConfigQuery : IRequest<MethodResult<TestConfigModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetTestConfigQueryHandler : IRequestHandler<GetTestConfigQuery, MethodResult<TestConfigModel>>
    {
        private readonly ITestConfigRepository _testConfigRepository;
        private readonly ITestConfigSectionRepository _testConfigSectionRepository;
        private readonly ISkillRepository _skillRepository;

        public GetTestConfigQueryHandler(
            ITestConfigRepository testConfigRepository,
            ITestConfigSectionRepository testConfigSectionRepository,
            ISkillRepository skillRepository)
        {
            _testConfigRepository = testConfigRepository;
            _testConfigSectionRepository = testConfigSectionRepository;
            _skillRepository = skillRepository;
        }

        public async Task<MethodResult<TestConfigModel>> Handle(GetTestConfigQuery request, CancellationToken cancellationToken)
        {
            MethodResult<TestConfigModel> methodResult = new MethodResult<TestConfigModel>();
            ArgumentNullException.ThrowIfNull(request);

            // 1. Lấy TestConfig + Program + Level
            var testConfigResult = await _testConfigRepository.Queryable
                .Include(x => x.Program)
                .Include(x => x.Level)
                .Where(tc => tc.Id == request.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (testConfigResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            // 2. Lấy toàn bộ Section theo TestConfigId
            var allSections = await _testConfigSectionRepository.Queryable
                .Where(s => s.TestConfigId == request.Id)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // 3. Lấy toàn bộ Skill
            var allSkills = await _skillRepository.Queryable
                .AsNoTracking()
                .ToDictionaryAsync(x => x.Id, cancellationToken);

            // 4. Mapping sang TestConfigModel
            var config = new TestConfigModel
            {
                Id = testConfigResult.Id,
                Name = testConfigResult.Name,
                Code = testConfigResult.Code,
                IsActive = testConfigResult.IsActive,
                ProgramId = testConfigResult.ProgramId,
                ProgramName = testConfigResult.Program?.Name,
                CreatedFullName = testConfigResult.CreatedFullName,
                CreatedDate = testConfigResult.CreatedDate,
                LevelId = testConfigResult.LevelId,
                LevelName = testConfigResult.Level?.Name,
                TestConfigSectionModels = BuildSectionTree(allSections, allSkills)
            };

            // 5. Trả kết quả
            methodResult.Result = config;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private List<TestConfigSectionModel> BuildSectionTree(List<TestConfigSection> sections, Dictionary<Guid, Skill>? skills = null, Guid? parentId = null)
        {
            return sections
                .Where(s => s.ParentId == parentId)
                .OrderBy(s => s.DisplayOrder)
                .Select(s => new TestConfigSectionModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    TargetWord = s.TargetWord,
                    DisplayOrder = s.DisplayOrder,
                    ExecutionTime = s.ExecutionTime,
                    LayoutType = s.LayoutType,
                    TotalScore = s.TotalScore,
                    Skill = s.Skill ?? (s.SkillId != null && skills != null && skills.TryGetValue(s.SkillId.Value, out var skill) ? skill : null),
                    ParentId = s.ParentId ?? Guid.Empty,
                    Config = s.Config,
                    Children = BuildSectionTree(sections, skills, s.Id)
                }).ToList();
        }
    }
}
