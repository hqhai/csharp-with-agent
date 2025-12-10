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

        public async Task Submit(Guid id)
        {
            await InitAggregate();
            var testResultComposite = TestResultComposites.FirstOrDefault(t => t.IsBelongTo(id));
            if (testResultComposite != null)
            {
                await testResultComposite.Submit();
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
                //var testService = ServiceProvider.GetRequiredService<ITestService>();
                if (!TestResult.SectionResults.Any())
                {
                    var testService = ServiceProvider.GetRequiredService<ITestService>();
                    await testService.MakeSectionTestResult(
                        SingleTestResult.StudentId!.Value,
                        TestResult,
                        SingleTestResult.ProgramId!.Value,
                        TestResult.TestId!.Value);

                    if (!SingleTestResult.TestResults.Contains(TestResult))
                    {
                        SingleTestResult.TestResults.Add(TestResult);
                    }

                    await AddNewTest(TestResult);
                }
                // var newTestResult = await testService.MakeSectionTestResult(SingleTestResult.StudentId!.Value, TestResult, SingleTestResult.ProgramId!.Value, TestResult.TestId!.Value);
                //
                // await AddNewTest(newTestResult);
            }

            if (!TestResultComposites.Any())
            {
                await InitAggregate();
            }

            var inprogressTestResult = TestResultComposites.FirstOrDefault(t => t.TestResult.Status == EnumResultStatus.Process);
            inprogressTestResult?.Start();

            await Commit();
        }

        private async Task AddNewTest(TestResult testResult)
        {
            //SingleTestResult.TestResults.Add(testResult);
            if (!SingleTestResult.TestResults.Contains(testResult))
            {
                SingleTestResult.TestResults.Add(testResult);
            }

            var testResultComposite = new TestResultComposite { Result = testResult, ServiceProvider = ServiceProvider };
            TestResultComposites.Add(testResultComposite);
            testResultComposite.GenerateChildren();
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
                TestStates = TestResultComposites.Select(c => c.ExportState()).ToList()
            };

            foreach (var testResult in singleTestStateModel.TestStates)
            {
                if (testResult is TestStateModel testStateModel)
                {
                    await UpdateTestResultDetailInfo(testStateModel);
                }
            }

            return singleTestStateModel;
        }

        public async Task MakeAnswers(SubmitAnswerCommandModel request)
        {
            var testService = ServiceProvider.GetRequiredService<ITestService>();
            await testService.CreateAnswers(request);

            if (request.IsSubmit)
            {
                await Submit(request.SectionResultId);
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
            var repository = ServiceProvider.GetRequiredService<IRepository<TestResult>>();
            if (repository.DbContext.ChangeTracker.HasChanges())
            {
                await repository.UnitOfWork.SaveChangesAsync();
            }
        }
    }
}
