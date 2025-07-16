// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Enums.ErrorCodes
{
    public enum EnumClassForumResultErrorCode
    {
        /// <summary>
        /// Class Forum result Status not pendding
        /// </summary>
        ClassForumResultStatusNotPendding,

        /// <summary>
        /// Class Forum result Status not pendding For Grading
        /// </summary>
        ClassForumResultStatusNotPendingForGrading,

        /// <summary>
        /// Can not delete in the current status
        /// </summary>
        CanNotDeleteInCurrentStatus,

        /// <summary>
        /// Class Forum result Status not graded
        /// </summary>
        ClassForumResultStatusNotGraded,

        /// <summary>
        /// Beed Back Star Can only Have 5 star
        /// </summary>
        FeedBackStarOnlyCanHane5,

        /// <summary>
        /// Result not from student
        /// </summary>
        ResultNotFromStudent,

        /// <summary>
        /// Feedback Positive Or FeedBack Both Have Value
        /// </summary>
        FeedbackPositiveOrFeedBackBothHaveValue,

        /// <summary>
        /// Cso Id Invalid
        /// </summary>
        CsoInvalid,

        /// <summary>
        /// Teacher Id Invalid
        /// </summary>
        TeacherInvalid,

        /// <summary>
        /// Contains Forbidden Keywords
        /// </summary>
        ContainsForbiddenKeywords,

        /// <summary>
        /// Class Forum result Status not pendding For Grading or Graded
        /// </summary>
        ClassForumResultStatusNotPendingForGradingOrGraded,

        /// <summary>
        /// Class Forum result Status not pendding For Grading or Graded
        /// </summary>
        ClassForumDetailHaveMoreThan2,

        /// <summary>
        /// Time's up for the ClassForum.
        /// </summary>
        TimeUpPostClassForum,

        /// <summary>
        /// Base64 In Text
        /// </summary>
        Base64InText,

        /// <summary>
        /// Tối đa số lần được phép pending speech to text
        /// </summary>
        MaxPendingSpeechToText,

        /// <summary>
        /// bài 1 AI chưa phản hồi thì ko được tạo bài 2
        /// </summary>
        AIPendingRecord1Record2CreationBlocked,

        /// <summary>
        /// class forum detail result not Status PendingSpeechToText
        /// </summary>
        RecordNotStatusPendingSpeechToText,

        /// <summary>
        /// đã quá 2 lần chỉnh sửa
        /// </summary>
        MaximumEditTwoClassForumDetail,

        /// <summary>
        /// word content null
        /// </summary>
        WordContentNull,
    }
}
