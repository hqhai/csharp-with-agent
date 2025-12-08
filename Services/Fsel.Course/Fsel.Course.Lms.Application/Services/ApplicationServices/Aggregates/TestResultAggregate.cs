// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.Aggregates
{
    using Core.Base.Interfaces;
    using Domain.Entities.TestConfigs;
    using Domain.Enums;
    using Domain.Models.CommandModels.Tests;
    using Domain.Models.EntityModels.PlacementTestModels;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;

    public class TestResultAggregate
    {
        public TestResult TestResult { get; set; }

        public IServiceProvider ServiceProvider { get; set; }

        public ICollection<TestResultComposite> TestResultComposites { get; set; } = new List<TestResultComposite>();

        public TestResultAggregate(TestResult testResult, IServiceProvider serviceProvider)
        {
            TestResult = testResult;
            ServiceProvider = serviceProvider;
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
            if (TestResult.Status == EnumResultStatus.Done)
            {
                return;
            }

            if (TestResult.Status == EnumResultStatus.New)
            {
                TestResult.Status = EnumResultStatus.Process;
            }

            if (!TestResult.SectionResults.Any() || TestResult.SectionResults.All(x => x.Status == EnumResultStatus.Done))
            {
                var testService = ServiceProvider.GetRequiredService<ITestService>();

                var newTestResult =
                    await testService.MakeNewTestResult(TestResult.StudentId, (Guid)TestResult.TestId!);

                await AddNewTest(newTestResult);
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
            //TestResult.TestResults.Add(testResult);
            var testResultComposite = new TestResultComposite() { Result = testResult, ServiceProvider = ServiceProvider };
            TestResultComposites.Add(testResultComposite);
            testResultComposite.GenerateChildren();
            await testResultComposite.LoadTotalScoreData();
            testResult.Status = EnumResultStatus.Process;
        }

        public async Task<TestStateModel> ExpotStateData()
        {
            var testResult = new TestStateModel
            {
                TestResultId = TestResult.Id,
                TestId = TestResult.TestId,
                Status = TestResult.Status,
                Children = TestResultComposites.Select(c => c.ExportState()).ToList()
            };

            foreach (var test in testResult.Children)
            {
                if (test is TestStateModel testStateModel)
                {
                    await UpdateTestResultDetailInfo(testStateModel);
                }
            }

            return testResult;
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

            foreach (var sectionResult in TestResult.SectionResults)
            {
                var testResultComposite = new TestResultComposite { Result = sectionResult, ServiceProvider = ServiceProvider };
                TestResultComposites.Add(testResultComposite);
                if (sectionResult.Status == EnumResultStatus.Process)
                {
                    var testService = ServiceProvider.GetRequiredService<ITestService>();
                    var hierarchicalTestResult = await testService.LoadHierachicalTestResult(x => x.Id == sectionResult.Id);
                    sectionResult.SectionResults = hierarchicalTestResult.SectionResults;
                    sectionResult.TestAnswers =  hierarchicalTestResult.TestAnswers;
                }

                testResultComposite.GenerateChildren();
            }
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
                    var sectionStateModel = new SectionStateModel
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
            var repository = ServiceProvider.GetRequiredService<IRepository<TestGroupResult>>();
            if (repository.DbContext.ChangeTracker.HasChanges())
            {
                await repository.UnitOfWork.SaveChangesAsync();
            }
        }
    }
}
