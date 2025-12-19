using Fsel.Course.Domain.Models.EntityModels;

namespace Fsel.Course.Lms.Application.Services.AIService.SpeakingAIService.Interface
{
    /// <summary>
    /// Interface định nghĩa các phương thức đánh giá phát âm với continuous recognition
    /// </summary>
    public interface IContinuousPronunciationAssessmentService
    {
        /// <summary>
        /// Đánh giá phát âm từ microphone với continuous recognition
        /// </summary>
        /// <param name="referenceText">Văn bản tham chiếu</param>
        /// <param name="unitId">ID của unit</param>
        /// <param name="cancellationToken">Token để hủy bỏ operation</param>
        /// <returns>Kết quả đánh giá phát âm</returns>
        Task<PronunciationAssessmentModel> AssessPronunciationFromMicrophoneContinuousAsync(string referenceText, Guid unitId = default, CancellationToken cancellationToken = default);

        /// <summary>
        /// Đánh giá phát âm từ file âm thanh với continuous recognition
        /// </summary>
        /// <param name="audioFilePath">Đường dẫn đến file âm thanh</param>
        /// <param name="referenceText">Văn bản tham chiếu</param>
        /// <param name="unitId">ID của unit</param>
        /// <param name="cancellationToken">Token để hủy bỏ operation</param>
        /// <returns>Kết quả đánh giá phát âm</returns>
        Task<PronunciationAssessmentModel> AssessPronunciationFromFileContinuousAsync(string audioFilePath, string referenceText, Guid unitId = default, CancellationToken cancellationToken = default);
    }
}