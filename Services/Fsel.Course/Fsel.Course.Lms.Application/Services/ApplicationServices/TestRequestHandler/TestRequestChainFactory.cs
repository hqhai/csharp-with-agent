// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.TestRequestHandler
{
    public interface ITestRequestChainFactory
    {
        IBaseTestRequestHandler GetTestSubmitRequestChainHandlers();
    }

    public class TestRequestChainFactory : ITestRequestChainFactory
    {
        private readonly ITestRequestGetCommonInfoHandler _testRequestGetCommonInfoHandler;
        private readonly ITestRequestValidateHandler _testRequestValidateHandler;
        private readonly ITestRequestCreateAnswerHandler _testRequestCreateAnswerHandler;
        private readonly ITestRequestSubmitHandler _testRequestSubmitHandler;
        private readonly ITestRequestStartNewModuleHandler _testRequestStartNewModuleHandler;

        public TestRequestChainFactory(ITestRequestGetCommonInfoHandler testRequestGetCommonInfoHandler,
            ITestRequestValidateHandler testRequestValidateHandler,
            ITestRequestCreateAnswerHandler testRequestCreateAnswerHandler,
            ITestRequestSubmitHandler testRequestSubmitHandler,
            ITestRequestStartNewModuleHandler testRequestStartNewModuleHandler)
        {
            _testRequestGetCommonInfoHandler = testRequestGetCommonInfoHandler;
            _testRequestValidateHandler = testRequestValidateHandler;
            _testRequestCreateAnswerHandler = testRequestCreateAnswerHandler;
            _testRequestSubmitHandler = testRequestSubmitHandler;
            _testRequestStartNewModuleHandler = testRequestStartNewModuleHandler;
        }

        public IBaseTestRequestHandler GetTestSubmitRequestChainHandlers()
        {
            _testRequestGetCommonInfoHandler.SetNext(_testRequestValidateHandler)
                .SetNext(_testRequestCreateAnswerHandler)
                .SetNext(_testRequestSubmitHandler)
                .SetNext(_testRequestStartNewModuleHandler);

            return _testRequestGetCommonInfoHandler;
        }
    }
}
