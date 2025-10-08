// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.LoadPTTestResultsHandler
{
    public interface ILoadPTResultChainFactory
    {
        IBaseLoadPTTestResultHandler GetLoadPTResultChainHandler(LoadDetailType loadDetailType = LoadDetailType.Question);
    }

    public class LoadPTResultChainFactory : ILoadPTResultChainFactory
    {
        private readonly ILoadQuestionDataHandler _loadQuestionDataHandler;
        private readonly ILoadTestExcerciseResultHandler _loadTestExcerciseResultHandler;
        private readonly ILoadTestGroupResultHandler _loadTestGroupResultHandler;
        private readonly ILoadTestResultHandler _loadTestResultHandler;
        private readonly ILoadTestSkillResultHandler _loadTestSkillResultHandler;

        public LoadPTResultChainFactory(ILoadQuestionDataHandler loadQuestionDataHandler,
            ILoadTestExcerciseResultHandler loadTestExcerciseResultHandler,
            ILoadTestGroupResultHandler loadTestGroupResultHandler,
            ILoadTestResultHandler loadTestResultHandler,
            ILoadTestSkillResultHandler loadTestSkillResultHandler)
        {
            _loadTestGroupResultHandler = loadTestGroupResultHandler;
            _loadTestResultHandler = loadTestResultHandler;
            _loadTestSkillResultHandler = loadTestSkillResultHandler;
            _loadTestExcerciseResultHandler = loadTestExcerciseResultHandler;
            _loadQuestionDataHandler = loadQuestionDataHandler;
        }

        public IBaseLoadPTTestResultHandler GetLoadPTResultChainHandler(LoadDetailType loadDetailType = LoadDetailType.Question)
        {
            switch (loadDetailType)
            {
                case LoadDetailType.Question:
                    _loadTestGroupResultHandler.SetNex(_loadTestResultHandler)
                        .SetNex(_loadTestSkillResultHandler)
                        .SetNex(_loadTestExcerciseResultHandler)
                        .SetNex(_loadQuestionDataHandler);
                    return _loadTestGroupResultHandler;

                case LoadDetailType.Test:
                    _loadTestGroupResultHandler.SetNex(_loadTestResultHandler);
                    return _loadTestGroupResultHandler;

                case LoadDetailType.Skill:
                    _loadTestGroupResultHandler.SetNex(_loadTestResultHandler)
                       .SetNex(_loadTestSkillResultHandler);
                    return _loadTestGroupResultHandler;

                case LoadDetailType.Excercise:
                    _loadTestGroupResultHandler.SetNex(_loadTestResultHandler)
                       .SetNex(_loadTestSkillResultHandler)
                       .SetNex(_loadTestExcerciseResultHandler);
                    return _loadTestGroupResultHandler;

                default:
                    return null;
            }
        }
    }

    public enum LoadDetailType
    {
        Test,
        Skill,
        Excercise,
        Question
    }
}
