// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.LoadPTTestResultsHandler
{
    using System.Threading.Tasks;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.Models.EntityModels.PlacementTestModels;
    using Microsoft.EntityFrameworkCore;

    public interface ILoadTestResultHandler : IBaseLoadPTTestResultHandler
    {
    }

    public class LoadTestResultHandler : BaseLoadPTTestResultHandler, ILoadTestResultHandler
    {
        private readonly IRepository<TestResult> _testResultRepository;

        public LoadTestResultHandler(IRepository<TestResult> testResultRepository)
        {
            _testResultRepository = testResultRepository;
        }

        public override async Task Handle(LoadPTTestResultContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            var testResults = await _testResultRepository.ReadQueryable
                 .Where(x => x.TestGroupResultId == context.PTState.TestGroupResultId)
            .ToListAsync();

            var branchesOfFlow = FlowService.GetAllFlowBranchesByFlow(context.FlowOfPT.StepFlows.FirstOrDefault()).ToList();

            context.NumberOfModules = branchesOfFlow.FirstOrDefault()?.Count;
            context.PTState.Modules = GetSequenceModulesMatch(branchesOfFlow, testResults);

            if (context.PTState.Modules == null || !context.PTState.Modules.Any())
            {
                return;
            }

            if (Next != null)
            {
                await Next.Handle(context);
            }
        }

        private static List<ModuleStateModel> GetSequenceModulesMatch(IEnumerable<List<ModuleStateModel>> branchesOfFlow, List<TestResult> testResults)
        {
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

                if (modules.Count == testResults.Count)
                {
                    return modules;
                }
            }

            return new List<ModuleStateModel>();
        }
    }
}
