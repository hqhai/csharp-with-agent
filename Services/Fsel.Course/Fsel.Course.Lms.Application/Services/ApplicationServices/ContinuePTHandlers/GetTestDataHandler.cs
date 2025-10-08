// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.ContinuePTHandlers
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.LoadPTTestResultsHandler;

    public interface IGetTestDataHandler : IBaseContinuePTHandler
    {
    }

    public class GetTestDataHandler : BaseContinuePTTestHandler, IGetTestDataHandler
    {
        private readonly ILoadPTResultChainFactory _loadPTResultChainFactory;

        public GetTestDataHandler(ILoadPTResultChainFactory loadPTResultChainFactory)
        {
            _loadPTResultChainFactory = loadPTResultChainFactory;
        }

        public override async Task Handle(ContinuePTTestContext context)
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
                StudentId = context.StudentId,
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
