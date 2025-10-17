// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Enums.ErrorCodes
{
    public enum EnumExamPracticeErrorCode
    {
        LockedClonedStatus,
        MissingRequiredData,
        DataChanged,
        RetryLimitExceeded,

        /// <summary>
        /// TestSessionInProgressOnAnotherDevice
        /// </summary>
        SessionOnOtherDevice,

        /// <summary>
        /// The test content has been updated. Please reload the page to get the latest version.
        /// </summary>
        TestStatusUpdated,

        /// <summary>
        /// This test has already been submitted from another device. Only the first submission is recorded. Please exit and view your result.
        /// </summary>
        TestAlreadySubmitted,

        /// <summary>
        /// This test was just submitted from another device. Please reload the page to view the latest result.
        /// </summary>
        TestJustSubmittedOnAnotherDevice,

        InvalidReadingQuestionCount,

        InvalidListenningQuestionCount,

        AlreadyExistsLearningData
    }
}
