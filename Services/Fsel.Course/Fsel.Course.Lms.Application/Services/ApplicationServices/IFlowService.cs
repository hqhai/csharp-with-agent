// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices
{
    using System;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Course.Domain.Entities.FlowConfigs;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels.PlacementTestModels;
    using Microsoft.EntityFrameworkCore;

    public interface IFlowService
    {
        Task<Flow> GetHierarchicalFlowByCondition(Expression<Func<Flow, bool>> predicate);

        Task<StepFlow> GetNextStepFlow(Guid testGroupResultId);

        Task PTStartNextModule(Guid studentId);

        Task<IEnumerable<List<ModuleStateModel>>> GetAllFlowBranches(Guid flowId);

        Task<IEnumerable<List<ModuleStateModel>>> GetBranchesMatch(Guid flowId, IEnumerable<TestResult> testResults);
    }

    public class FlowService : IFlowService
    {
        private readonly IFlowRepository _flowRepository;
        private readonly IStepFlowRepository _stepFlowRepository;
        private readonly ICategoryTestBankRepository _categoryTestBankRepository;
        private readonly ITestRepository _testRepository;
        private readonly IRepository<TestGroupResult> _testGroupResultRepository;
        private readonly ITestService _testService;

        public FlowService(IFlowRepository flowRepository,
            IStepFlowRepository stepFlowRepository,
            ICategoryTestBankRepository categoryTestBankRepository,
            ITestRepository testRepository,
            IRepository<TestGroupResult> testGroupResultRepository,
            ITestService testService)
        {
            _flowRepository = flowRepository;
            _stepFlowRepository = stepFlowRepository;
            _categoryTestBankRepository = categoryTestBankRepository;
            _testRepository = testRepository;
            _testGroupResultRepository = testGroupResultRepository;
            _testService = testService;
        }

        public async Task<IEnumerable<List<ModuleStateModel>>> GetAllFlowBranches(Guid flowId)
        {
            var flow = await GetHierarchicalFlowByCondition(x => x.Id == flowId);

            return GetAllFlowBranchesByFlow(flow.StepFlows.First());
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

        public Task<StepFlow> GetNextStepFlow(Guid testGroupResultId)
        {
            throw new NotImplementedException();
        }

        public async Task<Test> GetTestByStepFlow(StepFlow stepFlow, Guid programId)
        {
            var testIds = await _categoryTestBankRepository.ReadQueryable
                                        .Where(x => x.ProgramId == programId && x.TestType == Domain.Enums.EnumTestType.PlacementTest)
                                        .OrderBy(x => x.CreatedDate)
                                        .Select(x => x.TestOriginalId)
                                        .ToListAsync();

            var test = await _testRepository.ReadQueryable
                                .Where(x => x.LevelId == stepFlow.LevelId
                                    && testIds.Contains(x.OriginalId)
                                    && x.VersionStatus == Common.Enums.EnumVersionStatus.LastVersion)
                                .Include(x => x.TestSections)
                                .OrderBy(x => x.CreatedDate)
                                .FirstOrDefaultAsync();

            return test;
        }

        public async Task PTStartNextModule(Guid studentId)
        {
            var ptResult = await _testGroupResultRepository.ReadQueryable
                .Where(x => x.StudentId == studentId && x.TestType == Domain.Enums.EnumTestType.PlacementTest)
                .Include(x => x.TestResults)
                .FirstOrDefaultAsync();

            var allFlowBranch = await GetAllFlowBranches(ptResult.FlowId.Value);

            var branchMatchCurrentResult = GetBranchesMatch(allFlowBranch, ptResult.TestResults);

            var nextStepId = GetNextModule(branchMatchCurrentResult);

            await _testService.InitTestForStepFlow(studentId, nextStepId.Value, ptResult.Id, ptResult.ProgramId.Value);
        }

        public async Task<IEnumerable<List<ModuleStateModel>>> GetBranchesMatch(Guid flowId, IEnumerable<TestResult> testResults)
        {
            var branchesOfFlow = await GetAllFlowBranches(flowId);

            var matchBranches = new List<List<ModuleStateModel>>();

            foreach (var branch in branchesOfFlow)
            {
                foreach (var module in branch)
                {
                    var match = testResults.FirstOrDefault(x => x.StepFlowId == module.StepFlowId);
                    if (match == null)
                    {
                        break;
                    }

                    module.TestResultId = match.Id;
                    module.TestId = match.TestId;
                }

                var modules = branch.Where(x => x.TestResultId != null).ToList();

                if (modules.Count == testResults.Count())
                {
                    matchBranches.Add(branch);
                }
            }

            return matchBranches;
        }

        public static IEnumerable<List<ModuleStateModel>> GetBranchesMatch(IEnumerable<List<ModuleStateModel>> branchesOfFlow, IEnumerable<TestResult> testResults)
        {
            if (branchesOfFlow == null)
            {
                yield break;
            }

            foreach (var branch in branchesOfFlow)
            {
                foreach (var module in branch)
                {
                    var match = testResults.FirstOrDefault(x => x.StepFlowId == module.StepFlowId);
                    if (match == null)
                    {
                        break;
                    }

                    module.TestResultId = match.Id;
                    module.TestId = match.TestId;
                }

                var modules = branch.Where(x => x.TestResultId != null).ToList();

                if (modules.Count == testResults.Count())
                {
                    yield return branch;
                }
            }
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

        public static IEnumerable<List<ModuleStateModel>> GetAllFlowBranchesByFlow(StepFlow stepFlow)
        {
            if (stepFlow == null)
            {
                yield break;
            }

            if (stepFlow.ChildActionFlows == null || !stepFlow.ChildActionFlows.Any())
            {
                yield return new List<ModuleStateModel>()
                {
                    new ModuleStateModel
                    {
                        StepFlowId = stepFlow.Id,
                    },
                };
            }
            else
            {
                foreach (var actionFlow in stepFlow.ChildActionFlows)
                {
                    foreach (var modules in GetAllFlowBranchesByFlow(actionFlow.ToStepFlow))
                    {
                        var toModule = modules.FirstOrDefault();
                        if (toModule != null)
                        {
                            toModule.StartPercent = actionFlow.StartPercent;
                            toModule.ToPercent = actionFlow.EndPercent;
                        }
                        yield return new List<ModuleStateModel>()
                        {
                            new ModuleStateModel
                            {
                                StepFlowId = actionFlow.FromStepFlowId,
                            },
                        }.Concat(modules).ToList();
                    }
                }
            }
        }

        public Guid? GetNextModule(IEnumerable<List<ModuleStateModel>> branches)
        {
            var matchingBranchScore = branches.FirstOrDefault(b =>
            {
                for (var i = 0; i < b.Count; i++)
                {
                    if (b[i].TestResultId == null)
                    {
                        if (i == 0)
                        {
                            return true;
                        }
                        else
                        {
                            var previousModule = b[i - 1];
                            var currentModule = b[i];

                            return previousModule.PercentResult >= currentModule.StartPercent
                            && previousModule.PercentResult <= currentModule.ToPercent;
                        }
                    }
                }
                return false;
            });

            return matchingBranchScore?.FirstOrDefault()?.StepFlowId;
        }
    }
}
