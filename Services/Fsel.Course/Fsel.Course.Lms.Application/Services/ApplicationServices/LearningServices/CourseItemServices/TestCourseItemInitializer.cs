// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.LearningServices.CourseItemServices
{
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
    using Microsoft.EntityFrameworkCore;

    public class TestCourseItemInitializer : ICourseItemInitializer
    {
        private readonly ITestGroupResultRepository _testGroupResultRepository;
        private readonly ITestResultRepository _testResultRepository;
        private readonly ITestRepository _testRepository;

        public TestCourseItemInitializer(ITestGroupResultRepository testGroupResultRepository,
            ITestResultRepository testResultRepository,
            ITestRepository testRepository)
        {
            _testGroupResultRepository = testGroupResultRepository;
            _testResultRepository = testResultRepository;
            _testRepository = testRepository;
        }

        public async Task<VoidMethodResult> InitializeAsync(CourseModule courseModule, CourseResult courseResult, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(courseModule);
            ArgumentNullException.ThrowIfNull(courseResult);
            var methodResult = new VoidMethodResult();

            if (courseModule.CourseConfigType != EnumCourseConfigType.Test)
            {
                return methodResult;
            }

            var testGroupResult = await GetTestGroupResultAsync(courseResult.Id, courseModule.Id, cancellationToken);
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
                    }

                    testGroupResult.Status = EnumResultStatus.New;
                    await _testGroupResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
                }
                return methodResult;
            }

            var test = await _testRepository.ReadQueryable.Where(x => x.OriginalId == courseModule.OriginalId)
                                            .Where(x => x.VersionStatus == EnumVersionStatus.LastVersion)
                                            .FirstOrDefaultAsync(cancellationToken);

            if (test == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(test), courseModule.OriginalId);
                return methodResult;
            }
            await SaveTestGroupResultAsync(courseModule, test, courseResult);
            testGroupResult = await GetTestGroupResultAsync(courseResult.Id, courseModule.Id, cancellationToken);
            if (testGroupResult != null)
            {
                var testResult = new TestResult
                {
                    StudentId = courseResult.StudentId,
                    Status = EnumResultStatus.New,
                    NewDate = DateTime.UtcNow,
                    TestId = test.Id,
                    TestGroupResultId = testGroupResult.Id,
                };

                await _testResultRepository.BulkMergeAsync(new List<TestResult> { testResult }, bulk =>
                {
                    bulk.ColumnPrimaryKeyExpression = c => new { c.TestGroupResultId, c.TestId, c.StudentId, c.IsDeleted };
                });
            }

            return methodResult;
        }

        private async Task SaveTestGroupResultAsync(CourseModule courseModule, Test test, CourseResult courseResult)
        {
            var testGroupResult = new TestGroupResult
            {
                CourseId = courseResult.CourseId,
                CourseModuleId = courseModule.Id,
                CourseResultId = courseResult.Id,
                LevelId = test.LevelId,
                ProgramId = test.ProgramId,
                TestType = EnumTestType.FullTest,
                StudentId = courseResult.StudentId,
                Status = EnumResultStatus.New,
            };

            await _testGroupResultRepository.BulkMergeAsync(new List<TestGroupResult> { testGroupResult }, bulk =>
            {
                bulk.ColumnPrimaryKeyExpression = c => new { c.CourseModuleId, c.CourseResultId, c.IsDeleted };
            });
        }

        private async Task<TestGroupResult?> GetTestGroupResultAsync(Guid courseResultId, Guid courseModuleId, CancellationToken cancellationToken)
        {
            return await _testGroupResultRepository.Queryable.Include(x => x.TestResults)
                                                   .Where(x => x.CourseResultId == courseResultId)
                                                   .FirstOrDefaultAsync(x => x.CourseModuleId == courseModuleId, cancellationToken);
        }
    }
}
