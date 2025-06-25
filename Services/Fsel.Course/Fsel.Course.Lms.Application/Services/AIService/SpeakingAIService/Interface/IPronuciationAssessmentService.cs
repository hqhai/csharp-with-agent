using Fsel.Course.Domain.Models.EntityModels;

namespace Fsel.Course.Lms.Application.Services.AIService.SpeakingAIService.Interface
{
    /// <summary>
    /// Interface định nghĩa các phương thức đánh giá phát âm
    /// </summary>
    public interface IPronuciationAssessmentService
    {
        /// <summary>
        /// Đánh giá phát âm từ microphone
        /// </summary>
        /// <param name="referenceText">Văn bản tham chiếu</param>
        /// <returns>Kết quả đánh giá phát âm</returns>
        Task<PronunciationAssessmentModel> AssessPronunciationFromMicrophoneAsync(string referenceText);

        /// <summary>
        /// Đánh giá phát âm từ file âm thanh
        /// </summary>
        /// <param name="audioFilePath">Đường dẫn đến file âm thanh</param>
        /// <param name="referenceText">Văn bản tham chiếu</param>
        /// <returns>Kết quả đánh giá phát âm</returns>
        Task<PronunciationAssessmentModel> AssessPronunciationFromFileAsync(string audioFilePath, string referenceText);
    }
}
