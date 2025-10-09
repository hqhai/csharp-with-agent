// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.LoadPTTestResultsHandler
{
    public interface ILoadPTResultChainFactory
    {
        IBaseLoadPTTestResultHandler GetLoadPTResultChainHandler();
    }

    public class LoadPTResultChainFactory : ILoadPTResultChainFactory
    {
        private readonly ILoadQuestionDataHandler _loadQuestionDataHandler;
        private readonly ILoadTestSectionResultHandler _loadTestSectionResultHandler;
        private readonly ILoadTestGroupResultHandler _loadTestGroupResultHandler;
        private readonly ILoadTestResultHandler _loadTestResultHandler;

        public LoadPTResultChainFactory(ILoadQuestionDataHandler loadQuestionDataHandler,
            ILoadTestSectionResultHandler loadTestSectionResultHandler,
            ILoadTestGroupResultHandler loadTestGroupResultHandler,
            ILoadTestResultHandler loadTestResultHandler)
        {
            _loadTestGroupResultHandler = loadTestGroupResultHandler;
            _loadTestResultHandler = loadTestResultHandler;
            _loadTestSectionResultHandler = loadTestSectionResultHandler;
            _loadQuestionDataHandler = loadQuestionDataHandler;
        }

        public IBaseLoadPTTestResultHandler GetLoadPTResultChainHandler()
        {
            _loadTestGroupResultHandler.SetNex(_loadTestResultHandler)
                        .SetNex(_loadTestSectionResultHandler)
                        .SetNex(_loadQuestionDataHandler);
            return _loadTestGroupResultHandler;
        }
    }
}
