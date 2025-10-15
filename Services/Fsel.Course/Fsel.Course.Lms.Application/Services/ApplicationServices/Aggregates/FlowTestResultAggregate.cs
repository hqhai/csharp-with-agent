// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.Aggregates
{
    using System.Threading.Tasks;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Models.CommandModels.Tests;
    using Fsel.Course.Domain.Models.EntityModels.PlacementTestModels;
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
                await testResultComposite.Submit();
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

            if (FlowTestResult.TestResults == null || !FlowTestResult.TestResults.Any() || FlowTestResult.TestResults.All(x => x.Status == EnumResultStatus.Done))
            {
                var flowService = ServiceProvider.GetService<IFlowService>();
                var stepId = await flowService.GetNextStep(x => x.Id == FlowTestResult.FlowId, FlowTestResult.TestResults);

                if (stepId == null)
                {
                    var isDoneTest = FlowTestResult.TestResults.All(x => x.Status == EnumResultStatus.Done);
                    if (isDoneTest)
                    {
                        FlowTestResult.Status = EnumResultStatus.Done;
                        await Commit();
                    }
                    return;
                }
                var testService = ServiceProvider.GetService<ITestService>();

                var newTestResultTree = await testService.MakeNewTestResultTree(FlowTestResult.StudentId.Value, stepId.Value, FlowTestResult.Id, FlowTestResult.ProgramId.Value);

                await AddNewTest(newTestResultTree);
            }

            if (TestResultComposites == null || !TestResultComposites.Any())
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
            var testResultComposite = new TestResultComposite()
            {
                Result = testResult,
                ServiceProvider = ServiceProvider
            };
            TestResultComposites.Add(testResultComposite);
            testResultComposite.GenerateChildren();
            await testResultComposite.LoadTotalScoreData();
            testResult.Status = EnumResultStatus.Process;
        }

        public PTStateModel ExpotStateData()
        {
            return new PTStateModel
            {
                TestGroupResultId = FlowTestResult.Id,
                FlowId = FlowTestResult.FlowId,
                Status = FlowTestResult.Status,
                StudentId = FlowTestResult.StudentId,
                TestStates = TestResultComposites.Select(c => c.ExportState()).ToList()
            };
        }

        public async Task MakeAnswers(SubmitAnswerCommandModel request)
        {
            var testService = ServiceProvider.GetService<ITestService>();
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
                var testResultComposite = new TestResultComposite()
                {
                    Result = testResult,
                    ServiceProvider = ServiceProvider
                };
                TestResultComposites.Add(testResultComposite);
                if (testResult.Status == EnumResultStatus.Process)
                {
                    var testService = ServiceProvider.GetService<ITestService>();
                    var hierarchicalTestResult = await testService.LoadHierachicalTestResult(x => x.Id == testResult.Id);
                    testResult.SectionResults = hierarchicalTestResult.SectionResults;
                }

                testResultComposite.GenerateChildren();
            }
        }

        private async Task Commit()
        {
            var repository = ServiceProvider.GetService<IRepository<TestGroupResult>>();
            if (repository.DbContext.ChangeTracker.HasChanges())
            {
                await repository.UnitOfWork.SaveChangesAsync();
            }
        }
    }
}
