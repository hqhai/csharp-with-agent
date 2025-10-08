// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.TestRequestHandler
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.LoadPTTestResultsHandler;

    public interface ITestRequestGetTestDataHandler : IBaseTestRequestHandler
    {
    }

    public class TestRequestGetTestDataHandler : BaseTestRequestHandler, ITestRequestGetTestDataHandler
    {
        private readonly ILoadPTResultChainFactory _loadPTResultChainFactory;

        public TestRequestGetTestDataHandler(ILoadPTResultChainFactory loadPTResultChainFactory)
        {
            _loadPTResultChainFactory = loadPTResultChainFactory;
        }

        public override async Task Handle(TestRequestContext context)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));

            var loadPTResultChain = _loadPTResultChainFactory.GetLoadPTResultChainHandler();
            if (loadPTResultChain == null)
            {
                if (Next != null)
                {
                    await Next.Handle(context);
                }
                return;
            }

            var loadResultContext = new LoadPTTestResultContext
            {
                StudentId = context.TestRequestCommand.StudentId,
            };

            await loadPTResultChain.Handle(loadResultContext);

            context.PTState = loadResultContext.PTState;

            if (Next != null)
            {
                await Next.Handle(context);
            }
        }
    }
}
