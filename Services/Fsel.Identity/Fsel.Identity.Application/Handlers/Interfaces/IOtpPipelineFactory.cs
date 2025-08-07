// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Handlers.Interfaces
{
    public interface IOtpPipelineFactory
    {
        IOtpHandlerPipeline CreatePipeline(OtpStep otpStep);
    }
}
