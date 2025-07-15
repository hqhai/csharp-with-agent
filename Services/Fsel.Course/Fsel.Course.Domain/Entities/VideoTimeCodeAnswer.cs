// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Enums.ErrorCodes;

    public class VideoTimeCodeAnswer : BaseAnswer
    {
        public bool IsFirstSubmit { get; set; }
        public Question? Question { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid QuestionId { get; set; }

        public Exercise? Exercise { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid ExerciseId { get; set; }

        public VideoTimeCode? VideoTimeCode { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid VideoTimeCodeId { get; set; }

        public VideoResult? VideoResult { get; set; }
        public Guid? VideoResultId { get; set; }

        public VideoTimeCodeResult? VideoTimeCodeResult { get; set; }
        public Guid? VideoTimeCodeResultId { get; set; }

        public int TokenReceived { get; set; }
    }
}
