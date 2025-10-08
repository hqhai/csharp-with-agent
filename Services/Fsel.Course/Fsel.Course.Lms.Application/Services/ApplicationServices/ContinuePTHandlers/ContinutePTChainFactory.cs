// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.ContinuePTHandlers
{
    public interface IContinutePTChainFactory
    {
        IBaseContinuePTHandler GeContinutePTChainHandler();
    }

    public class ContinutePTChainFactory : IContinutePTChainFactory
    {
        private readonly IGetCurrentStateHandler _getCurrentStateHandler;
        private readonly IGetTestDataHandler _getTestDataHandler;
        private readonly IInitForNextTestHandler _initForNextTestHandler;

        public ContinutePTChainFactory(IGetCurrentStateHandler getCurrentStateHandler,
            IGetTestDataHandler getTestDataHandler,
            IInitForNextTestHandler initForNextTestHandler)
        {
            _getCurrentStateHandler = getCurrentStateHandler;
            _getTestDataHandler = getTestDataHandler;
            _initForNextTestHandler = initForNextTestHandler;
        }

        public IBaseContinuePTHandler GeContinutePTChainHandler()
        {
            _getCurrentStateHandler.SetNex(_initForNextTestHandler)
                .SetNex(_getTestDataHandler);

            return _getCurrentStateHandler;
        }
    }
}
