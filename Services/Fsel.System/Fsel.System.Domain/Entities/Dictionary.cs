// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Entities
{
    using Fsel.Common.Helpers;
    using Fsel.Core.Entities;
    using Fsel.System.Domain.Models.CommandModels.Dictionaries;
    using global::System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// Từ điển Anh - Việt
    /// </summary>
    public class Dictionary : Entity
    {
        /// <summary>
        /// Từ tiếng anh
        /// </summary>
        public string? Word { get; set; }

        /// <summary>
        /// Phiên âm
        /// </summary>
        public string? Phonetic { get; set; }

        /// <summary>
        /// Loại từ
        /// </summary>
        public string? PartOfSpeech { get; set; }

        /// <summary>
        /// Nghĩa tiếng việt
        /// </summary>
        public string? Meaning { get; set; }

        /// <summary>
        /// Danh sách ví dụ
        /// </summary>
        [NotMapped]
        public IList<Example>? Examples
        {
            get => ConvertHelper.Deserialize<IList<Example>>(ExampleStr);
            set => ExampleStr = ConvertHelper.Serialize(value);
        }

        /// <summary>
        /// Chuỗi danh sách ví dụ
        /// </summary>
        public string? ExampleStr { get; set; }
    }
}
