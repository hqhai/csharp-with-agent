// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.TestQuery
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels.SkillModels;
    using Fsel.Course.Domain.Models.EntityModels.TestModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetTestQuery : IRequest<MethodResult<TestModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetTestQueryHandler : IRequestHandler<GetTestQuery, MethodResult<TestModel>>
    {
        private readonly ITestRepository _testRepository;
        private readonly ITestSectionRepository _testSectionRepository;
        private readonly ISkillRepository _skillRepository;
        private readonly IMapper _mapper;

        public GetTestQueryHandler(
            ITestRepository testRepository,
            ITestSectionRepository testSectionRepository,
            ISkillRepository skillRepository,
            IMapper mapper)
        {
            _testRepository = testRepository;
            _testSectionRepository = testSectionRepository;
            _skillRepository = skillRepository;
            _mapper = mapper;
        }

        public async Task<MethodResult<TestModel>> Handle(GetTestQuery request, CancellationToken cancellationToken)
        {
            MethodResult<TestModel> methodResult = new MethodResult<TestModel>();
            ArgumentNullException.ThrowIfNull(request);

            // 1. Lấy Test + Program + Level
            var test = await _testRepository.Queryable
                .Include(x => x.Program)
                .Include(x => x.Level)
                .Where(tc => tc.Id == request.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (test == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            // 2. Lấy toàn bộ Section theo TestId
            var allSections = await _testSectionRepository.Queryable
                .Where(s => s.TestId == request.Id)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // 3. Lấy toàn bộ Skill
            var allSkills = await _skillRepository.Queryable
                .AsNoTracking()
                .ToDictionaryAsync(x => x.Id, cancellationToken);

            // 4. Mapping sang TestModel
            var config = _mapper.Map<TestModel>(test);
            config.TestSectionModels = BuildSectionTree(allSections, allSkills);

            // 5. Trả kết quả
            methodResult.Result = config;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private List<TestSectionModel> BuildSectionTree(
                List<TestSection> sections,
                Dictionary<Guid, Skill>? skills = null,
                Guid? parentId = null)
        {
            var children = sections
                .Where(s => s.ParentId == parentId)
                .OrderBy(s => s.DisplayOrder)
                .ToList();

            var mappedChildren = _mapper.Map<List<TestSectionModel>>(children);

            foreach (var child in mappedChildren)
            {
                var original = children.First(x => x.Id == child.Id);
                // Gán Skill từ Dictionary nếu chưa có
                child.Skill = _mapper.Map<SkillModel>(original.Skill ?? (original.SkillId != null && skills != null && skills.TryGetValue(original.SkillId.Value, out var skill) ? skill : null));
                // Gán Children đệ quy
                child.Childrens = BuildSectionTree(sections, skills, child.Id);
            }

            return mappedChildren;
        }
    }
}
