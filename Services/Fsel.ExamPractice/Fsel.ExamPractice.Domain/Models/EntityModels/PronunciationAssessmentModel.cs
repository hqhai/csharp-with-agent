// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Models.EntityModels
{
    using System.Collections.Generic;
    using System.Linq;
    using Fsel.Shared.Enums;

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
                        FilePath = word.FilePath,
                        AccuracyScore = word.AccuracyScore,
                        Syllables = word.Syllables,
                        Color = word.Color,
                        Phonemes = word.Phonemes
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

        public EnumSubmissionCount? SubmissionCount { get; set; }
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
        /// Từ
        /// </summary>
        public string? FilePath { get; set; }

        /// <summary>
        /// Điểm chính xác
        /// </summary>
        public double AccuracyScore { get; set; }

        /// <summary>
        /// Danh sách các syllable trong từ
        /// </summary>
        public IList<SyllableInfo>? Syllables { get; set; }

        /// <summary>
        /// Danh sách các phoneme trong từ
        /// </summary>
        public IList<PhonemeInfo>? Phonemes { get; set; }

        /// <summary>
        /// Mã màu hiển thị (đỏ, vàng, xanh) dựa trên điểm chính xác
        /// </summary>
        public EnumSyllableMarked Color { get; set; }
    }

    /// <summary>
    /// Thông tin syllable
    /// </summary>
    public class SyllableInfo
    {
        /// <summary>
        /// Âm tiết
        /// </summary>
        public string? Syllable { get; set; }

        /// <summary>
        /// Âm tiết theo ký hiệu IPA
        /// </summary>
        public string? IPASyllable { get; set; }

        /// <summary>
        /// Điểm chính xác
        /// </summary>
        public double AccuracyScore { get; set; }

        /// <summary>
        /// Vị trí bắt đầu của âm tiết trong audio
        /// </summary>
        public long Offset { get; set; }

        /// <summary>
        /// Thời lượng của âm tiết
        /// </summary>
        public long Duration { get; set; }

        /// <summary>
        /// Từ của âm
        /// </summary>
        public string? Grapheme { get; set; }

        /// <summary>`w1
        /// Mã màu hiển thị (đỏ, vàng, xanh) dựa trên điểm chính xác
        /// </summary>
        public EnumSyllableMarked Color { get; set; }
    }

    public enum EnumSyllableMarked
    {
        Green,
        Yellow,
        Red
    }

    /// <summary>
    /// Thông tin phoneme
    /// </summary>
    public class PhonemeInfo
    {
        /// <summary>
        /// Âm vị
        /// </summary>
        public string? Phoneme { get; set; }

        /// <summary>
        /// Điểm chính xác
        /// </summary>
        public double AccuracyScore { get; set; }

        /// <summary>
        /// Vị trí bắt đầu của âm vị trong audio
        /// </summary>
        public long Offset { get; set; }

        /// <summary>
        /// Thời lượng của âm vị
        /// </summary>
        public long Duration { get; set; }
    }
}
