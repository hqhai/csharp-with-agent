// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.ContinuePTHandlers
{
    using System.Threading.Tasks;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.Enums;
    using Microsoft.EntityFrameworkCore;

    public interface IGetCurrentStateHandler : IBaseContinuePTHandler
    {
    }

    public class GetCurrentStateHandler : BaseContinuePTTestHandler, IGetCurrentStateHandler
    {
        private readonly IRepository<TestGroupResult> _flowTestResultRepository;

        public GetCurrentStateHandler(IRepository<TestGroupResult> flowTestResultRepository)
        {
            _flowTestResultRepository = flowTestResultRepository;
        }

        public override async Task Handle(ContinuePTTestContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            var flowTestResult = await _flowTestResultRepository.ReadQueryable.Where(x => x.StudentId == context.StudentId
            && x.TestType == EnumTestType.PlacementTest)
                .Include(x => x.TestResults)
                .FirstOrDefaultAsync();
            if (flowTestResult != null)
            {
                if (flowTestResult.Status == EnumResultStatus.Done)
                {
                    context.CurrentState = CurrentState.Done;
                    return;
                }
                else if (flowTestResult.Status is EnumResultStatus.Process or EnumResultStatus.New)
                {
                    var testResults = flowTestResult.TestResults;
                    context.CurrentState = testResults == null || !testResults.Any() || testResults.All(x => x.Status == EnumResultStatus.Done)
                        ? CurrentState.StartNewTest
                        : CurrentState.TestInprogress;
                }
            }
            else
            {
                context.CurrentState = CurrentState.NotTestYet;
                return;
            }

            if (Next != null)
            {
                await Next.Handle(context);
            }
        }
    }
}
