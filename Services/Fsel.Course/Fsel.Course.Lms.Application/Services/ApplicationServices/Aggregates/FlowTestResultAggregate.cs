// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.Aggregates
{
    using System.Threading.Tasks;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.Graphs;

    public class FlowTestResultAggregate
    {
        public TestGroupResult FlowTestResult { get; set; }

        public IServiceProvider ServiceProvider { get; set; }

        public List<TestResultComposite> TestResultComposites { get; set; }

        public async Task Submit(Guid id)
        {
            var testResultComposite = TestResultComposites?.FirstOrDefault(t => t.IsBelongTo(id));
            if (testResultComposite != null)
            {
                await testResultComposite.Submit();
            }

            await Commit();

            await Start();
        }

        public async Task Start()
        {
            var inprogressTestResult = TestResultComposites.FirstOrDefault(t => t.TestResult.Status == EnumResultStatus.Process);
            if (inprogressTestResult != null)
            {
                inprogressTestResult.Start();
            }
            else
            {
                var flowService = ServiceProvider.GetService(typeof(IFlowService)) as IFlowService;
                var flow = await flowService.GetHierarchicalFlowByCondition(x => x.Id == FlowTestResult.FlowId);

                var startNode = Node.CreateStartNode(flow.StepFlows.First());

                foreach (var testResult in FlowTestResult.TestResults)
                {
                    startNode.AssignStepResult(testResult);
                }

                var node = startNode.TestResult == null ? startNode : startNode.GetNextNode();

                if (node != null)
                {
                    var testService = ServiceProvider.GetService(typeof(ITestService)) as ITestService;

                    await testService.InitTestForStepFlow(FlowTestResult.StudentId.Value, node.StepFlow.Id, FlowTestResult.Id, FlowTestResult.ProgramId.Value);
                }
            }
        }

        public async Task Load()
        {
        }

        public async Task MakeAnswer()
        {
        }

        public async Task InitAggregate()
        {
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
                    var testService = ServiceProvider.GetService(typeof(ITestService)) as ITestService;
                    var hierarchicalTestResult = await testService.LoadHierachicalTestResult(x => x.Id == testResult.Id);
                    testResult.SectionResults = hierarchicalTestResult.SectionResults;
                }

                testResultComposite.GenerateChildren();
            }
        }

        private async Task Commit()
        {
            var repository = ServiceProvider.GetService(typeof(IRepository<TestGroupResult>)) as IRepository<TestGroupResult>;
            await repository.UnitOfWork.SaveEntitiesAsync();
        }

        public static FlowTestResultAggregate Create(TestGroupResult flowTestResult)
        {
            return new FlowTestResultAggregate()
            {
                FlowTestResult = flowTestResult
            };
        }
    }
}
