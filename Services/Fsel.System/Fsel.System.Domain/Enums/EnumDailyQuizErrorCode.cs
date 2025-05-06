namespace Fsel.System.Domain.Enums
{
    using global::System.ComponentModel;

    public enum EnumDailyQuizErrorCode
    {
        [Description("Thiếu cấu hình sự kiện")]
        MissingEventConfiguration,

        [Description("Sự kiện đã hết hạn hoặc chưa diễn ra")]
        EventExpiredOrNotYetOccurred,

        [Description("Đã hoàn thành ngày Daily Quiz hôm nay")]
        CompletedDailyQuizToday,

        [Description("Số lượng câu hỏi đã hết")]
        NumberOfQuestionsOut,

        [Description("Đã trả lời câu hỏi này")]
        AnsweredThisQuestion,

        [Description("Sai số lượng câu hỏi")]
        WrongNumberOfQuestions,

        [Description("Không thuộc sự kiện")]
        NotPartOfTheEvent,

        [Description("Câu hỏi không tồn tại")]
        QuestionDoesNotExist,

        [Description("Câu trả lời không tồn tại")]
        AnswerDoesNotExist,

        [Description("Đã hết thời gian làm bài")]
        TimeIsUp,

        [Description("Bị trùng câu hỏi")]
        DuplicateQuestion
    }
}
