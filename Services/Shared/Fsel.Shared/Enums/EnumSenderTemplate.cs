// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Shared.Enums
{
    public enum EnumSenderTemplate
    {
        SendOtp,
        SendOtpAndLink,
        SendStudentPTOnline,
        SendSurveyToParentStudent,
        SendStudentCompleteUnitGood,
        SendStudentCompleteUnitWeak,
        SendStudentCompleteCourseIetls,
        SendStudentCompleteCourseAcademic,
        Unit1Report,
        Unit2AboveReport,
        WeeklyReport,
        WeeklyReport2,
        WeeklyReport3,
        WeeklyReport4,
        StudentCompletePT,
        SendMailMidCourseAcademic,
        SendMailMidCourseIELT,
        PaymentApproval,
        MailPaymentForStudent,
        MailPaymentForCustomer,
        MailFromLandingPage,
        CreateUserForEventULIS,
        MailPaymentWithVoucher,
        CreateUser,
        MailRegisterForEventHaNoi,
        MailRegisterForEventPhuTho,
        MailRegisterForEventUlis,
        MailRegisterForEventPhenikaa,

        #region event

        CreateAccountWithEventSuccess,
        SignUpEventSuccess,
        WasInAnotherEvent,
        LearnedOnThePlatform,
        NotEligibleToParticipate,
        MailRegisterEvent

        #endregion event
    }
}