// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.TestConfigQuery
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
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
        private readonly IMapper _mapper;

        public GetTestConfigQueryHandler(
            ITestConfigRepository testConfigRepository,
            ITestConfigSectionRepository testConfigSectionRepository,
            ISkillRepository skillRepository,
            IMapper mapper)
        {
            _testConfigRepository = testConfigRepository;
            _testConfigSectionRepository = testConfigSectionRepository;
            _skillRepository = skillRepository;
            _mapper = mapper;
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
            var config = _mapper.Map<TestConfigModel>(testConfigResult);
            config.TestConfigSectionModels = BuildSectionTree(allSections, allSkills);

            // 5. Trả kết quả
            methodResult.Result = config;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private List<TestConfigSectionModel> BuildSectionTree(
                List<TestConfigSection> sections,
                Dictionary<Guid, Skill>? skills = null,
                Guid? parentId = null)
        {
            var children = sections
                .Where(s => s.ParentId == parentId)
                .OrderBy(s => s.DisplayOrder)
                .ToList();

            var mappedChildren = _mapper.Map<List<TestConfigSectionModel>>(children);

            foreach (var child in mappedChildren)
            {
                var original = children.First(x => x.Id == child.Id);
                // Gán Skill từ Dictionary nếu chưa có
                child.Skill = original.Skill ?? (original.SkillId != null && skills != null && skills.TryGetValue(original.SkillId.Value, out var skill) ? skill : null);
                // Gán Children đệ quy
                child.Children = BuildSectionTree(sections, skills, child.Id);
            }

            return mappedChildren;
        }

    }
}
