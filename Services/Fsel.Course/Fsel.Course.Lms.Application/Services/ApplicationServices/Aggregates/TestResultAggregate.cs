// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.Aggregates
{
    using Core.Base.Interfaces;
    using Domain.Entities.TestConfigs;
    using Domain.Enums;
    using Domain.Models.CommandModels.Tests;
    using Domain.Models.EntityModels.TestModels;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using BaseTestStateModel = Domain.Models.EntityModels.PlacementTestModels.BaseTestStateModel;
    using SectionStateModel = Domain.Models.EntityModels.PlacementTestModels.SectionStateModel;
    using TestStateModel = Domain.Models.EntityModels.PlacementTestModels.TestStateModel;

    public class TestResultAggregate
    {
        public TestGroupResult SingleTestResult { get; set; }
        public IServiceProvider ServiceProvider { get; set; }
        public TestResult TestResult { get; set; }

        public ICollection<TestResultComposite> TestResultComposites { get; set; } = new List<TestResultComposite>();

        public TestResultAggregate(TestGroupResult singleTestResult, IServiceProvider serviceProvider, TestResult testResult)
        {
            SingleTestResult = singleTestResult;
            ServiceProvider = serviceProvider;
            TestResult = testResult;
        }

        public async Task SubmitTest(Guid id)
        {
            await InitAggregate();
            var testResultComposite = TestResultComposites.FirstOrDefault(t => t.IsBelongTo(id));
            if (testResultComposite != null)
            {
                await testResultComposite.SubmitTest(new SubmitContext { Id = id, ScoringFormulaType = TestResult.Test?.ScoringFormulaType });
                if (TestResult.Status == EnumResultStatus.Done)
                {
                    SingleTestResult.Status = EnumResultStatus.Done;
                    await CommitTest();
                    await CommitTestGroup();
                }
                else
                {
                    await Commit();
                }
            }
        }

        public async Task Submit(Guid id)
        {
            await InitAggregate();
            var testResultComposite = TestResultComposites.FirstOrDefault(t => t.IsBelongTo(id));
            if (testResultComposite != null)
            {
                await testResultComposite.Submit(new SubmitContext { Id = id, ScoringFormulaType = TestResult.Test?.ScoringFormulaType });
                await Commit();
            }

            await Start();
        }

        public async Task Start()
        {
            if (SingleTestResult.Status == EnumResultStatus.Done)
            {
                return;
            }

            if (SingleTestResult.Status == EnumResultStatus.New)
            {
                SingleTestResult.Status = EnumResultStatus.Process;
            }

            if (!SingleTestResult.TestResults.Any() || SingleTestResult.TestResults.All(x => x.Status == EnumResultStatus.Done))
            {
                if (!TestResult.SectionResults.Any())
                {
                    var testService = ServiceProvider.GetRequiredService<ITestService>();
                    var testResultWithSection = await testService.MakeSectionTestResult(
                        SingleTestResult.StudentId!.Value,
                        TestResult,
                        TestResult.TestId!.Value);

                    await AddNewTest(testResultWithSection);
                }
            }

            if (!TestResultComposites.Any())
            {
                await InitAggregate();
            }

            var inprogressTestResult = TestResultComposites.FirstOrDefault(t => t.TestResult.Status == EnumResultStatus.Process);
            inprogressTestResult?.Start();

            await Commit();
        }

        #region Test

        public async Task StartTest()
        {
            if (SingleTestResult.Status == EnumResultStatus.Done)
            {
                return;
            }

            if (SingleTestResult.Status == EnumResultStatus.New)
            {
                SingleTestResult.Status = EnumResultStatus.Process;
            }

            if (TestResult.Status == EnumResultStatus.New)
            {
                TestResult.Status = EnumResultStatus.Process;
                if (!TestResult.SectionResults.Any())
                {
                    var testService = ServiceProvider.GetRequiredService<ITestService>();
                    var testResultWithSection = await testService.MakeSectionTestResult(
                        SingleTestResult.StudentId!.Value,
                        TestResult,
                        TestResult.TestId!.Value);

                    await AddNewTest(testResultWithSection);
                }
            }

            if (!TestResultComposites.Any())
            {
                await InitAggregateTest();
            }

            var inprogressTestResult = TestResultComposites.FirstOrDefault(t => t.TestResult.Status == EnumResultStatus.Process);
            inprogressTestResult?.Start();

            await Commit();
        }

        public async Task InitAggregateTest()
        {
            if (TestResultComposites.Any())
            {
                return;
            }

            var testResultComposite = new TestResultComposite { Result = TestResult, ServiceProvider = ServiceProvider };
            TestResultComposites.Add(testResultComposite);
            if (TestResult.Status == EnumResultStatus.Process)
            {
                var testService = ServiceProvider.GetRequiredService<ITestService>();
                var hierarchicalTestResult = await testService.LoadHierachicalTestResult(x => x.Id == TestResult.Id);
                TestResult.SectionResults = hierarchicalTestResult.SectionResults;
                TestResult.TestAnswers = hierarchicalTestResult.TestAnswers;
                await testResultComposite.LoadTestHierarchicalData();
            }

            testResultComposite.GenerateChildren();
        }

        #endregion Test

        private async Task AddNewTest(TestResult testResult)
        {
            var testResultComposite = new TestResultComposite { Result = testResult, ServiceProvider = ServiceProvider };
            TestResultComposites.Add(testResultComposite);
            testResultComposite.GenerateChildren();
            await testResultComposite.LoadTestHierarchicalData();
            await testResultComposite.LoadTotalScoreData();
            testResult.Status = EnumResultStatus.Process;
        }

        public async Task<SingleTestStateModel> ExpotStateData()
        {
            var singleTestStateModel = new SingleTestStateModel
            {
                TestGroupResultId = SingleTestResult.Id,
                StudentId = SingleTestResult.StudentId,
                Status = SingleTestResult.Status,
                Level = SingleTestResult.LevelId?.ToString(),
                TestStates = TestResultComposites.Select(c => c.ExportForTestState()).ToList()
            };

            foreach (var testResult in singleTestStateModel.TestStates)
            {
                if (testResult is TestStateModel testStateModel)
                {
                    await UpdateTestResultDetailInfo(testStateModel);
                    testStateModel.Children = testStateModel.Children.Cast<SectionStateModel>().OrderBy(x => x.Order).Cast<BaseTestStateModel>().ToList();
                }
            }

            return singleTestStateModel;
        }

        public async Task MakeAnswers(SubmitAnswerCommandModel request)
        {
            ArgumentNullException.ThrowIfNull(request);
            var testService = ServiceProvider.GetRequiredService<ITestService>();
            await testService.CreateAnswers(request);

            if (request.IsSubmit)
            {
                await Submit(request.SectionResultId);
            }
        }

        public async Task MakeTestAnswers(SubmitAnswerCommandModel request)
        {
            ArgumentNullException.ThrowIfNull(request);
            var testService = ServiceProvider.GetRequiredService<ITestService>();
            await testService.CreateTestAnswers(request);

            if (request.IsSubmit)
            {
                await SubmitTest(request.SectionResultId);
            }
        }

        public async Task InitAggregate()
        {
            if (TestResultComposites.Any())
            {
                return;
            }

            var testResultComposite = new TestResultComposite { Result = TestResult, ServiceProvider = ServiceProvider };
            TestResultComposites.Add(testResultComposite);
            if (TestResult.Status == EnumResultStatus.Process)
            {
                var testService = ServiceProvider.GetRequiredService<ITestService>();
                var hierarchicalTestResult = await testService.LoadHierachicalTestResult(x => x.Id == TestResult.Id);
                TestResult.SectionResults = hierarchicalTestResult.SectionResults;
                TestResult.TestAnswers = hierarchicalTestResult.TestAnswers;
            }
            testResultComposite.GenerateChildren();
            await testResultComposite.LoadTestHierarchicalData();
        }

        public async Task UpdateTestResultDetailInfo(TestStateModel? testStateModel)
        {
            if (testStateModel?.TestId == null)
            {
                return;
            }

            var testService = ServiceProvider.GetRequiredService<ITestService>();
            var testSectionResultRepository = ServiceProvider.GetRequiredService<IRepository<TestSectionResult>>();
            var test = await testService.GetHierachicalTestById(testStateModel.TestId.Value);

            testStateModel.UpdateDetailInfo(test);

            if (testStateModel.Status == EnumResultStatus.Done && !testStateModel.Children.Any())
            {
                var testSectionResults = await testSectionResultRepository.ReadQueryable
                    .Include(x => x.TestSection)
                    .ThenInclude(x => x!.Skill)
                    .Where(x => x.TestResultId == testStateModel.TestResultId && x.ParentTestSectionResultId == null)
                    .ToListAsync();

                testStateModel.Children = testSectionResults.Select(BaseTestStateModel (skill) =>
                {
                    var sectionStateModel = new SectionStateModel()
                    {
                        Name = skill.TestSection?.Skill?.Name,
                        TestLayoutType = skill.TestSection?.LayoutType,
                        FilePath = skill.TestSection?.Skill?.FilePath,
                        HighestStreak = skill.HighestStreak,
                        PercentResult = skill.Percent,
                        CurrentSectionTimeCodeId = skill.CurrentSectionTimeCodeId,
                        SkillScores = skill.SkillScores,
                        SectionResultId = skill.Id,
                        CorrectCount = skill.CorrectCount,
                        TotalCount = skill.CorrectTotal,
                        Status = skill.Status,
                        UpdatedDate = skill.UpdatedDate ?? skill.CreatedDate
                    };

                    return sectionStateModel;
                }).ToList();
            }
        }

        private async Task Commit()
        {
            var repositoryTestResult = ServiceProvider.GetRequiredService<IRepository<TestResult>>();
            if (repositoryTestResult.DbContext.ChangeTracker.HasChanges())
            {
                await repositoryTestResult.UnitOfWork.SaveChangesAsync();
            }
        }

        private async Task CommitTest()
        {
            var repositoryTestResult = ServiceProvider.GetRequiredService<IRepository<TestResult>>();
            if (repositoryTestResult.DbContext.ChangeTracker.HasChanges())
            {
                await repositoryTestResult.UnitOfWork.SaveChangesAsync();
            }
        }

        private async Task CommitTestGroup()
        {
            var repositoryTestGroupResult = ServiceProvider.GetRequiredService<IRepository<TestGroupResult>>();
            repositoryTestGroupResult.Update(SingleTestResult);
            await repositoryTestGroupResult.UnitOfWork.SaveEntitiesAsync();
        }
    }
}
