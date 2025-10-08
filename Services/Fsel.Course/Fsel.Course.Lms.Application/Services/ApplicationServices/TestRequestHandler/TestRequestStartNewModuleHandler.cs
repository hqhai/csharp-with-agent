// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.TestRequestHandler
{
    using System.Threading.Tasks;

    public interface ITestRequestStartNewModuleHandler : IBaseTestRequestHandler
    {
    }

    public class TestRequestStartNewModuleHandler : BaseTestRequestHandler, ITestRequestStartNewModuleHandler
    {
        private readonly IFlowService _flowService;

        public TestRequestStartNewModuleHandler(IFlowService flowService)
        {
            _flowService = flowService;
        }

        public override async Task Handle(TestRequestContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            if (context.TestRequestCommand?.IsSubmit == true && context.StartNewModule)
            {
                ArgumentNullException.ThrowIfNull(context);
                await _flowService.PTStartNextModule(context.Student.Id);

                if (Next != null)
                {
                    await Next.Handle(context);
                }
            }
        }
    }
}
