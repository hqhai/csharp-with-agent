// Copyright (c) Atlantic. All rights reserved.

using Fsel.Course.Domain.Entities.QuestionTypeConfigs;

namespace Fsel.Course.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Common.Helpers;
    using Fsel.Core.Entities;

    public class QuestionShuffle : Entity
    {
        public Question? Question { get; set; }

        [Required(ErrorMessage = nameof(EnumSystemErrorCode.Required))]
        public Guid QuestionId { get; set; }

        public Guid StudentId { get; set; }
        public string? ShuffleConfigStr { get; set; }

        [NotMapped]
        public IList<SubQuestionConfig>? ShuffleConfigs
        {
            get { return ShuffleConfigStr.Deserialize<IList<SubQuestionConfig>>(); }
            set { ShuffleConfigStr = value.Serialize(); }
        }
    }
}
