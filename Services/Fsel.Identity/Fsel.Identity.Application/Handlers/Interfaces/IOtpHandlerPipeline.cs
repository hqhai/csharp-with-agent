// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Handlers.Interfaces
{
    public interface IOtpHandlerPipeline
    {
        Task Handle(OtpPipelineContext context);

        IOtpHandlerPipeline Next { get; set; }

        IOtpHandlerPipeline SetNext(IOtpHandlerPipeline next);
    }

    public interface IOtpHandlerPipeline<T> : IOtpHandlerPipeline where T : IOtpHandlerPipeline
    {
    }

    public abstract class BaseOtpHandlerPipeline : IOtpHandlerPipeline
    {
        public IOtpHandlerPipeline Next { get; set; }

        public abstract Task Handle(OtpPipelineContext context);

        public IOtpHandlerPipeline SetNext(IOtpHandlerPipeline next)
        {
            Next = next;
            return Next;
        }
    }

    public class OtpPipelineContext
    {
        public OtpPipelineContext(string phoneNumber, OtpPurpose purpose, OtpStep step)
        {
            PhoneNumber = phoneNumber;
            Purpose = purpose;
            Step = step;
        }

        public string PhoneNumber { get; }
        public OtpPurpose Purpose { get; }
        public OtpProviderType OtpProviderType { get; set; } = OtpProviderType.Sms;
        public OtpStep Step { get; set; }

        public int MaxCountVerifyFail { get; set; }
        public int? MaxCountOtpSend { get; set; }

        public TimeSpan? OtpBlockDuration { get; set; }
        public TimeSpan? BlockSendOtpDuration { get; set; }
        public TimeSpan? MinimumBetweenTwoSendsDuration { get; set; }
        public TimeSpan OtpLifeTimeDuration { get; set; }

        public string? Otp { get; set; }
        public string? RequestOtp { get; set; }
        public bool Status { get; set; }
        public string? ErrorMessage { get; set; }

        public string BlockedOtpCacheKey => $"{PhoneNumber}:{Purpose}:Block_Send_And_Verify_Otp";
        public string BlockedSendOtpCacheKey => $"{PhoneNumber}:{Purpose}:Otp_Send_Otp";
        public string CountFailedVerifyOtpCacheKey => $"{PhoneNumber}:{Purpose}:Failed_Verify_Otp_Count";
        public string CountSendOtpCacheKey => $"{PhoneNumber}:{Purpose}:Send_Otp_Count";
        public string OtpCacheKey => $"{PhoneNumber}:{Purpose}:Otp";
    }

    public enum OtpPurpose
    {
        Register,
        Login,
        ResetPassword,
        ChangePhoneNumber
    }

    public enum OtpProviderType
    {
        Sms,
        Email,
        Zalo,
    }

    public enum OtpStep
    {
        SendOtp,
        VerifyOtp
    }

    public class SendOtpCountInfo
    {
        public int Count { get; set; }
        public DateTime LastSendTime { get; set; }
        public DateTime StartTime { get; set; }
    }

    public class FailedCountInfo
    {
        public int Count { get; set; }
        public DateTime LastFailedTime { get; set; }
        public DateTime StartTime { get; set; }
    }
}
