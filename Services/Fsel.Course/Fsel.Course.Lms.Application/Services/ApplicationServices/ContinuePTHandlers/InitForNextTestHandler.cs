// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.ContinuePTHandlers
{
    using System.Threading.Tasks;

    public interface IInitForNextTestHandler : IBaseContinuePTHandler
    {
    }

    public class InitForNextTestHandler : BaseContinuePTTestHandler, IInitForNextTestHandler
    {
        private readonly IFlowService _flowService;

        public InitForNextTestHandler(IFlowService flowService)
        {
            _flowService = flowService;
        }

        public override async Task Handle(ContinuePTTestContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            if (context.CurrentState == CurrentState.StartNewTest)
            {
                await _flowService.PTStartNextModule(context.StudentId);
            }

            if (Next != null)
            {
                await Next.Handle(context);
            }
        }
    }
}
