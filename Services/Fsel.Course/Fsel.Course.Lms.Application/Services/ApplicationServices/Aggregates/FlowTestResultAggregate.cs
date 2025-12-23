// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.Aggregates
{
    using System.Threading.Tasks;
    using Core.Base.Interfaces;
    using Domain.Entities.TestConfigs;
    using Domain.Enums;
    using Domain.IRepositories;
    using Domain.Models.CommandModels.Tests;
    using Domain.Models.EntityModels.PlacementTestModels;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;

    public class FlowTestResultAggregate
    {
        public TestGroupResult FlowTestResult { get; set; }

        public IServiceProvider ServiceProvider { get; set; }

        public ICollection<TestResultComposite> TestResultComposites { get; set; } = new List<TestResultComposite>();

        public FlowTestResultAggregate(TestGroupResult testGroupResult, IServiceProvider serviceProvider)
        {
            FlowTestResult = testGroupResult;
            ServiceProvider = serviceProvider;
        }

        public async Task Submit(Guid id)
        {
            await InitAggregate();
            var testResultComposite = TestResultComposites?.FirstOrDefault(t => t.IsBelongTo(id));
            if (testResultComposite != null)
            {
                await testResultComposite.Submit(id);
                await Commit();
            }

            await Start();
        }

        public async Task Start()
        {
            if (FlowTestResult.Status == EnumResultStatus.Done)
            {
                return;
            }

            if (FlowTestResult.Status == EnumResultStatus.New)
            {
                FlowTestResult.Status = EnumResultStatus.Process;
            }

            if (!FlowTestResult.TestResults.Any() || FlowTestResult.TestResults.All(x => x.Status == EnumResultStatus.Done))
            {
                var flowService = ServiceProvider.GetRequiredService<IFlowService>();
                var node = await flowService.GetNextStep(x => x.Id == FlowTestResult.FlowId, FlowTestResult.TestResults);

                if (node?.StepFlow?.Id == null || node?.IsLeft == true)
                {
                    FlowTestResult.CurrentLevelId = node?.StepFlow?.LevelId;
                    FlowTestResult.Status = EnumResultStatus.Done;
                    await Commit();
                    return;
                }

                var testService = ServiceProvider.GetRequiredService<ITestService>();

                var newTestResultTree =
                    await testService.MakeNewTestResultTree(FlowTestResult.StudentId.Value, node.StepFlow.Id, FlowTestResult.Id, FlowTestResult.ProgramId.Value);

                await AddNewTest(newTestResultTree);
            }

            if (!TestResultComposites.Any())
            {
                await InitAggregate();
            }

            var inprogressTestResult = TestResultComposites.FirstOrDefault(t => t.TestResult.Status == EnumResultStatus.Process);
            inprogressTestResult?.Start();

            await Commit();
        }

        public async Task AddNewTest(TestResult testResult)
        {
            FlowTestResult.TestResults.Add(testResult);
            var testResultComposite = new TestResultComposite() { Result = testResult, ServiceProvider = ServiceProvider };
            TestResultComposites.Add(testResultComposite);
            testResultComposite.GenerateChildren();
            await testResultComposite.LoadTotalScoreData();
            testResult.Status = EnumResultStatus.Process;
        }

        public async Task<PtStateModel> ExpotStateData()
        {
            var ptResult = new PtStateModel
            {
                TestGroupResultId = FlowTestResult.Id,
                FlowId = FlowTestResult.FlowId,
                Status = FlowTestResult.Status,
                StudentId = FlowTestResult.StudentId,
                Level = FlowTestResult.LevelId?.ToString(),
                TestStates = TestResultComposites.Select(c => c.ExportState()).ToList()
            };

            foreach (var testResult in ptResult.TestStates)
            {
                if (testResult is TestStateModel testStateModel)
                {
                    await UpdateTestResultDetailInfo(testStateModel);
                }
            }

            return ptResult;
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

            foreach (var testResult in FlowTestResult.TestResults)
            {
                var testResultComposite = new TestResultComposite() { Result = testResult, ServiceProvider = ServiceProvider };
                TestResultComposites.Add(testResultComposite);
                if (testResult.Status == EnumResultStatus.Process)
                {
                    var testService = ServiceProvider.GetRequiredService<ITestService>();
                    var hierarchicalTestResult = await testService.LoadHierachicalTestResult(x => x.Id == testResult.Id);
                    testResult.SectionResults = hierarchicalTestResult.SectionResults;
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

            if (testStateModel?.Status == EnumResultStatus.Done && !testStateModel.Children.Any())
            {
                var testSectionResults = await testSectionResultRepository.ReadQueryable
                    .Include(x => x.TestSection)
                    .ThenInclude(x => x.Skill)
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
                        UpdatedDate = skill?.UpdatedDate ?? skill?.CreatedDate
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
