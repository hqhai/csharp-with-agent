// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.LearningServices.UnitItemServices
{
    using System.Linq.Dynamic.Core;
    using System.Threading;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.Entities.V1i1;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Infrastructure.Repositories;
    using Microsoft.EntityFrameworkCore;

    public class TestUnitItemInitializer : IUnitItemInitializer
    {
        private readonly ITestGroupResultRepository _testGroupResultRepository;
        private readonly ITestResultRepository _testResultRepository;
        private readonly ITestRepository _testRepository;

        public TestUnitItemInitializer(ITestGroupResultRepository testGroupResultRepository,
            ITestResultRepository testResultRepository,
            ITestRepository testRepository)
        {
            _testGroupResultRepository = testGroupResultRepository;
            _testResultRepository = testResultRepository;
            _testRepository = testRepository;
        }

        public async Task<VoidMethodResult> InitializeAsync(UnitModule unitModule, UnitResult unitResult, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(unitModule);
            ArgumentNullException.ThrowIfNull(unitResult);
            var methodResult = new VoidMethodResult();

            if (unitModule.UnitConfigType != EnumUnitConfigType.Test)
            {
                return methodResult;
            }

            var testGroupResult = await GetTestGroupResultAsync(unitResult.Id, unitModule.Id, cancellationToken);
            if (testGroupResult != null)
            {
                if (testGroupResult.Status == EnumResultStatus.Unfinished)
                {
                    foreach (var item in testGroupResult.TestResults)
                    {
                        if (item.Status != EnumResultStatus.Unfinished)
                        {
                            continue;
                        }
                        item.Status = EnumResultStatus.New;
                        item.NewDate = DateTime.UtcNow;
                    }

                    testGroupResult.Status = EnumResultStatus.New;
                    await _testGroupResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
                }

                return methodResult;
            }

            var test = await _testRepository.ReadQueryable.Where(x => x.OriginalId == unitModule.OriginalId)
                                         .Where(x => x.VersionStatus == EnumVersionStatus.LastVersion)
                                         .FirstOrDefaultAsync(cancellationToken);

            if (test == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(test), unitModule.OriginalId);
                return methodResult;
            }
            await SaveTestGroupResultAsync(unitModule, test, unitResult);
            testGroupResult = await GetTestGroupResultAsync(unitResult.Id, unitModule.Id, cancellationToken);
            if (testGroupResult != null)
            {
                var testResult = new TestResult
                {
                    StudentId = unitResult.StudentId,
                    Status = EnumResultStatus.New,
                    NewDate = DateTime.UtcNow,
                    TestId = test.Id,
                    TestGroupResultId = testGroupResult.Id,
                };

                await _testResultRepository.BulkMergeAsync(new List<TestResult> { testResult }, bulk =>
                {
                    bulk.ColumnPrimaryKeyExpression = c => new { c.TestGroupResultId, c.TestId, c.IsDeleted };
                });
            }

            return methodResult;
        }

        private async Task SaveTestGroupResultAsync(UnitModule unitModule, Test test, UnitResult unitResult)
        {
            var testGroupResult = new TestGroupResult
            {
                UnitModuleId = unitModule.Id,
                UnitResultId = unitResult.Id,
                CourseId = unitResult.CourseId,
                CourseModuleId = unitResult.CourseModuleId,
                CourseResultId = unitResult.CourseResultId,
                LevelId = test.LevelId,
                ProgramId = test.ProgramId,
                UnitId = unitResult.UnitId,
                TestType = EnumTestType.SkillTest,
                StudentId = unitResult.StudentId,
                Status = EnumResultStatus.New,
            };

            await _testGroupResultRepository.BulkMergeAsync(new List<TestGroupResult> { testGroupResult }, bulk =>
            {
                bulk.ColumnPrimaryKeyExpression = c => new { c.UnitModuleId, c.UnitResultId, c.IsDeleted };
            });
        }

        private async Task<TestGroupResult?> GetTestGroupResultAsync(Guid unitResultId, Guid unitModuleId, CancellationToken cancellationToken)
        {
            return await _testGroupResultRepository.Queryable.Where(x => x.UnitResultId == unitResultId)
                                                   .FirstOrDefaultAsync(x => x.UnitModuleId == unitModuleId, cancellationToken);
        }
    }
}
