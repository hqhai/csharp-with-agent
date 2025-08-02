// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Handlers.Implementations
{
    using Fsel.Identity.Application.Handlers.Interfaces;

    public class OtpPipelineFactory : IOtpPipelineFactory
    {
        private readonly IOtpHandlerPipeline<CheckBlockOtpHandler> _checkBlockOtpHandler;
        private readonly IOtpHandlerPipeline<CheckBlockSendOtpHandler> _checkBlockSendOtpHandler;
        private readonly IOtpHandlerPipeline<SendOtpHandler> _sendOtpHandler;
        private readonly IOtpHandlerPipeline<SendOtpResultHandler> _sendOtpResultHandler;
        private IOtpHandlerPipeline<VerifyOtpHandler> _verifyOtpHandler;
        private readonly IOtpHandlerPipeline<VerifyOtpResultHandler> _verifyOtpResultHandler;
        private readonly IOtpHandlerPipeline<OtpInfoCollectHandler> _otpInfoCollectHandler;

        public OtpPipelineFactory(IOtpHandlerPipeline<CheckBlockOtpHandler> checkBlockOtpHandler,
            IOtpHandlerPipeline<CheckBlockSendOtpHandler> checkBlockSendOtpHandler,
            IOtpHandlerPipeline<SendOtpHandler> sendOtpHandler,
            IOtpHandlerPipeline<SendOtpResultHandler> sucessSendOtpHandler,
            IOtpHandlerPipeline<VerifyOtpHandler> verifyOtpHandler,
            IOtpHandlerPipeline<VerifyOtpResultHandler> failedVerifyOtpHandler,
            IOtpHandlerPipeline<OtpInfoCollectHandler> otpInfoCollectHandler)
        {
            _checkBlockOtpHandler = checkBlockOtpHandler;
            _checkBlockSendOtpHandler = checkBlockSendOtpHandler;
            _sendOtpHandler = sendOtpHandler;
            _sendOtpResultHandler = sucessSendOtpHandler;
            _verifyOtpHandler = verifyOtpHandler;
            _verifyOtpResultHandler = failedVerifyOtpHandler;
            _otpInfoCollectHandler = otpInfoCollectHandler;
        }

        public IOtpHandlerPipeline CreatePipeline(OtpStep otpStep)
        {
            IOtpHandlerPipeline startNode = _otpInfoCollectHandler;
            if (otpStep == OtpStep.VerifyOtp)
            {
                startNode.SetNext(_checkBlockOtpHandler)
                .SetNext(_verifyOtpHandler)
                .SetNext(_verifyOtpResultHandler);
            }
            else if (otpStep == OtpStep.SendOtp)
            {
                startNode.SetNext(_checkBlockOtpHandler)
                .SetNext(_checkBlockSendOtpHandler)
                .SetNext(_sendOtpHandler)
                .SetNext(_sendOtpResultHandler);
            }
            return startNode;
        }
    }
}
