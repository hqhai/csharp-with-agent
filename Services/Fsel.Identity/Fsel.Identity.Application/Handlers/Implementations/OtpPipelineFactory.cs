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

        public OtpPipelineFactory(IOtpHandlerPipeline<CheckBlockOtpHandler> checkBlockOtpHandler,
            IOtpHandlerPipeline<CheckBlockSendOtpHandler> checkBlockSendOtpHandler,
            IOtpHandlerPipeline<SendOtpHandler> sendOtpHandler,
            IOtpHandlerPipeline<SendOtpResultHandler> sucessSendOtpHandler,
            IOtpHandlerPipeline<VerifyOtpHandler> verifyOtpHandler,
            IOtpHandlerPipeline<VerifyOtpResultHandler> failedVerifyOtpHandler)
        {
            _checkBlockOtpHandler = checkBlockOtpHandler;
            _checkBlockSendOtpHandler = checkBlockSendOtpHandler;
            _sendOtpHandler = sendOtpHandler;
            _sendOtpResultHandler = sucessSendOtpHandler;
            _verifyOtpHandler = verifyOtpHandler;
            _verifyOtpResultHandler = failedVerifyOtpHandler;
        }

        public IOtpHandlerPipeline CreatePipeline(OtpStep otpStep)
        {
            IOtpHandlerPipeline startNode = null;
            if (otpStep == OtpStep.VerifyOtp)
            {
                startNode = _checkBlockOtpHandler;
                startNode.SetNext(_verifyOtpHandler)
                .SetNext(_verifyOtpResultHandler);
            }
            else if (otpStep == OtpStep.SendOtp)
            {
                startNode = _checkBlockOtpHandler;
                startNode.SetNext(_checkBlockSendOtpHandler)
                .SetNext(_sendOtpHandler)
                .SetNext(_sendOtpResultHandler);
            }
            return startNode;
        }
    }
}
