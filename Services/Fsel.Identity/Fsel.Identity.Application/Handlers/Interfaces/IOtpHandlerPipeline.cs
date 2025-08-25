// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Identity.Application.Handlers.Interfaces
{
    using Fsel.Shared.Constants;
    using Z.BulkOperations;

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
        public OtpPipelineContext(string identity, OtpPurpose purpose, OtpStep step)
        {
            Identity = identity;
            Purpose = purpose;
            Step = step;

            OtpBlockDuration = OtpSetting.OtpBlockDuration;
            OtpLifeTimeDuration = OtpSetting.OtpLifeTimeDuration;
            GapSendDuration = OtpSetting.GapSendDuration;
            SendOtpCountLifeTimeDuration = OtpSetting.SendOtpCountLifeTimeDuration;
            MaxCountOtpSend = OtpSetting.MaxCountOtpSend;
            MaxCountVerifyFail = OtpSetting.MaxCountVerifyFail;
        }

        public string Identity { get; }
        public OtpPurpose Purpose { get; }
        public OtpProviderType OtpProviderType { get; set; } = OtpProviderType.Sms;
        public OtpStep Step { get; set; }

        public int MaxCountVerifyFail { get; set; }
        public int? MaxCountOtpSend { get; set; }

        public TimeSpan? OtpBlockDuration { get; set; }
        public TimeSpan? SendOtpCountLifeTimeDuration { get; set; }
        public TimeSpan? GapSendDuration { get; set; }
        public TimeSpan OtpLifeTimeDuration { get; set; }

        public string? Otp { get; set; }
        public string? RequestOtp { get; set; }
        public bool Status { get; set; }
        public KeyValuePair<string, string>? ErrorMessage { get; set; }


        public OtpSessionInfo OtpSessionInfo { get; set; } = new OtpSessionInfo();

        public string BlockedOtpCacheKey => $"{Identity}:{Purpose}:Block_Send_And_Verify_Otp";
        public string BlockedSendOtpCacheKey => $"{Identity}:{Purpose}:Otp_Send_Otp";
        public string CountFailedVerifyOtpCacheKey => $"{Identity}:{Purpose}:Failed_Verify_Otp_Count";
        public string CountSendOtpCacheKey => $"{Identity}:{Purpose}:Send_Otp_Count";
        public string OtpCacheKey => $"{Identity}:{Purpose}:Otp";
    }

    public enum OtpPurpose
    {
        Register,
        Login,
        Forgot
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
        VerifyOtp,
        CollectData
    }

    public enum FailType
    {
        Blocked,
        BlockedResend,
        GapResend,

        Expired,
        VerifyInvalid
    }

    public class SendOtpCountInfo
    {
        public int Count { get; set; }
        public string LastProvider { get; set; }
        public DateTime LastSendTime { get; set; }
        public DateTime StartTime { get; set; }
    }

    public class FailedCountInfo
    {
        public int Count { get; set; }
        public DateTime LastFailedTime { get; set; }
        public DateTime StartTime { get; set; }
    }


    public class OtpSessionInfo
    {
        public bool OtpExpired { get; set; }
        public bool IsOtpBlocked { get; set; }
        public TimeSpan? WaitTimeDuration { get; set; }
        public SendInfo? SendInfo { get; set; }
        public VerifyInfo? VerifyInfo { get; set; }

        public bool CanSendDirectly => SendInfo == null || (!SendInfo.IsBlockedByReachMaxSendCount && !SendInfo.IsBlockedByGap);
    }

    public class SendInfo
    {
        public bool IsJustSendLastTime { get; set; }
        public bool IsBlockedByReachMaxSendCount { get; set; }
        public bool IsBlockedByGap { get; set; }
        public TimeSpan? WaitTimeDuration { get; set; }
        public string Provider { get; set; }
    }

    public class VerifyInfo
    {
        public int VerifyFailCount { get; set; }
    }
}
