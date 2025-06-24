// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.TestConfigQuery
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels.TestConfig;
    using Fsel.Course.Infrastructure.Repositories;
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
        private readonly ITestLayoutRepository _testLayoutRepository;
        private readonly ISkillRepository _skillRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ISkillLevelRepository _skillLevelRepository;
        private readonly IStepFlowRepository _stepFlowRepository;
        private readonly IPlacementTestRepository _placementTestRepository;
        private readonly ILevelRepository _levelRepository;

        public GetTestConfigQueryHandler(ITestConfigRepository testConfigRepository, ISkillRepository skillRepository,
            ICategoryRepository categoryRepository, ITestLayoutRepository testLayoutRepository, ISkillLevelRepository skillLevelRepository,
            IStepFlowRepository stepFlowRepository, IPlacementTestRepository placementTestRepository, ILevelRepository levelRepository)
        {
            _testConfigRepository = testConfigRepository;
            _skillRepository = skillRepository;
            _categoryRepository = categoryRepository;
            _testLayoutRepository = testLayoutRepository;
            _skillLevelRepository = skillLevelRepository;
            _stepFlowRepository = stepFlowRepository;
            _placementTestRepository = placementTestRepository;
            _levelRepository = levelRepository;
        }

        public async Task<MethodResult<TestConfigModel>> Handle(GetTestConfigQuery request, CancellationToken cancellationToken)
        {
            MethodResult<TestConfigModel> methodResult = new MethodResult<TestConfigModel>();
            ArgumentNullException.ThrowIfNull(request);
            var testConfig = _testConfigRepository.Queryable;
            var skill = _skillRepository.Queryable;
            var category = _categoryRepository.Queryable;
            var testLayout = _testLayoutRepository.Queryable;
            var testConfigEntity = await (from tc in testConfig
                                          where tc.Id == request.Id && !tc.IsDeleted
                                          select tc)
                             .FirstOrDefaultAsync(cancellationToken);

            if (testConfigEntity == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }
            #region Skill
            var skillResult = await skill.Where(x => !x.IsDeleted && x.SkillLevels.Any(z => z.Id == x.Id)).ToListAsync(cancellationToken);
            if (skillResult.Any())
            {
                testConfigEntity.Skills = skillResult;
            }

            #endregion
            #region Category
            #endregion Category

            //methodResult.Result = testConfigEntity;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
