using Fsel.Shared.Enums;

namespace Fsel.Course.Domain.Models.EntityModels
{
    /// <summary>
    /// Kết quả đánh giá phát âm
    /// </summary>
    public class PronunciationAssessmentModel
    {
        private List<SpokenWord>? _spokenWords;

        /// <summary>
        /// Điểm phát âm tổng hợp
        /// </summary>
        public int PronunciationScore { get; set; }

        /// <summary>
        /// Điểm chính xác
        /// </summary>
        public double AccuracyScore { get; set; }

        /// <summary>
        /// Điểm trôi chảy
        /// </summary>
        public double? FluencyScore { get; set; }

        /// <summary>
        /// Điểm ngữ điệu
        /// </summary>
        public double? ProsodyScore { get; set; }

        /// <summary>
        /// Văn bản được nhận dạng
        /// </summary>
        public string? Transcription { get; set; }

        /// <summary>
        /// Danh sách các từ cần cải thiện
        /// </summary>
        public List<SpokenWord>? SpokenWords
        {
            get => _spokenWords;
            set
            {
                if (value != null)
                {
                    _spokenWords = value.Select(word => new SpokenWord
                    {
                        Word = word.Word,
                        AccuracyScore = word.AccuracyScore,
                        Color = word.Color,
                    }).ToList();
                }
                else
                {
                    _spokenWords = null;
                }
            }
        }

        /// <summary>
        /// Thông báo lỗi nếu có
        /// </summary>
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Kết quả cải thiện từ
    /// </summary>
    public class SpokenWord
    {
        /// <summary>
        /// Từ
        /// </summary>
        public string? Word { get; set; }

        /// <summary>
        /// Điểm chính xác
        /// </summary>
        public double AccuracyScore { get; set; }

        /// <summary>
        /// Mã màu hiển thị (đỏ, vàng, xanh)
        /// </summary>
        public EnumSyllableMarked? Color { get; set; }
    }
}
