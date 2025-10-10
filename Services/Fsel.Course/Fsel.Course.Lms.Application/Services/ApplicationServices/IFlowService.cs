// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices
{
    using System;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Fsel.Course.Domain.Entities.FlowConfigs;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.Graphs;
    using Microsoft.EntityFrameworkCore;

    public interface IFlowService
    {
        Task<Flow> GetHierarchicalFlowByCondition(Expression<Func<Flow, bool>> predicate);

        Task<Guid?> GetNextStep(Expression<Func<Flow, bool>> predicate, ICollection<TestResult> testResults);
    }

    public class FlowService : IFlowService
    {
        private readonly IFlowRepository _flowRepository;
        private readonly IStepFlowRepository _stepFlowRepository;

        public FlowService(IFlowRepository flowRepository,
            IStepFlowRepository stepFlowRepository)
        {
            _flowRepository = flowRepository;
            _stepFlowRepository = stepFlowRepository;
        }

        public async Task<Flow> GetHierarchicalFlowByCondition(Expression<Func<Flow, bool>> predicate)
        {
            var flow = await _flowRepository.ReadQueryable
                           .Include(x => x.StepFlows)
                           .ThenInclude(x => x.ChildActionFlows.OrderBy(af => af.CreatedDate))
                           .Include(x => x.StepFlows)
                           .ThenInclude(x => x.Level)
                           .Where(predicate)
                           .OrderBy(x => x.CreatedDate)
                           .FirstOrDefaultAsync();
            if (flow != null)
            {
                foreach (var stepFlow in flow.StepFlows)
                {
                    await LoadStepFlowRecursively(stepFlow);
                }
            }

            return flow;
        }

        public async Task<Guid?> GetNextStep(Expression<Func<Flow, bool>> predicate, ICollection<TestResult>? testResults)
        {
            var flow = await GetHierarchicalFlowByCondition(predicate);

            var startNode = Node.CreateStartNode(flow.StepFlows.First());

            if (testResults != null)
            {
                foreach (var testResult in testResults)
                {
                    startNode.AssignStepResult(testResult);
                }
            }

            var node = startNode.TestResult == null ? startNode : startNode.GetNextNode();

            return node?.StepFlow?.Id;
        }

        private async Task LoadStepFlowRecursively(StepFlow stepFlow)
        {
            if (stepFlow?.ChildActionFlows == null || !stepFlow.ChildActionFlows.Any())
            {
                return;
            }

            foreach (var actionFlow in stepFlow.ChildActionFlows)
            {
                actionFlow.ToStepFlow = await _stepFlowRepository.ReadQueryable
                                                 .Include(x => x.Level)
                                                 .Include(x => x.ChildActionFlows.OrderBy(af => af.CreatedDate))
                                                 .FirstOrDefaultAsync(sf => sf.Id == actionFlow.ToStepFlowId);

                if (actionFlow.ToStepFlow != null)
                {
                    if (actionFlow.ToStepFlow.Type == Domain.Enums.EnumStepFlowType.End)
                    {
                        actionFlow.ToStepFlow.ParentActionFlows.Clear();
                    }

                    await LoadStepFlowRecursively(actionFlow.ToStepFlow);
                }
            }
        }
    }
}
