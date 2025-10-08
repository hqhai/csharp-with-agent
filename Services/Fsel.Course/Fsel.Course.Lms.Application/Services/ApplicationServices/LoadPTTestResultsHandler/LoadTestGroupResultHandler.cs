// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.LoadPTTestResultsHandler
{
    using System.Threading.Tasks;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.Models.EntityModels.PlacementTestModels;
    using Microsoft.EntityFrameworkCore;

    public interface ILoadTestGroupResultHandler : IBaseLoadPTTestResultHandler
    {
    }

    public class LoadTestGroupResultHandler : BaseLoadPTTestResultHandler, ILoadTestGroupResultHandler
    {
        private readonly IRepository<TestGroupResult> _testGroupResultRepository;
        private readonly IFlowService _flowService;

        public LoadTestGroupResultHandler(IRepository<TestGroupResult> testGroupResultRepository,
            IFlowService flowService)
        {
            _testGroupResultRepository = testGroupResultRepository;
            _flowService = flowService;
        }

        public override async Task Handle(LoadPTTestResultContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            var testGroupResult = await _testGroupResultRepository.ReadQueryable.Where(x => x.StudentId == context.StudentId
            && x.TestType == Domain.Enums.EnumTestType.PlacementTest)
                .FirstOrDefaultAsync();

            if (testGroupResult == null)
            {
                return;
            }

            context.PTState = new PTStateModel
            {
                FlowId = testGroupResult.FlowId,
                StudentId = testGroupResult.StudentId,
                TestGroupResultId = testGroupResult.Id,
                Status = testGroupResult.Status
            };

            context.FlowOfPT = await _flowService.GetHierarchicalFlowByCondition(f => f.Id == testGroupResult.FlowId);
            context.ProgramId = context.FlowOfPT.ProgramId;
            if (Next != null)
            {
                await Next.Handle(context);
            }
        }
    }
}
