// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Domain.Entities
{
    using System.ComponentModel.DataAnnotations;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Core.Entities;
    using Fsel.ExamPractice.Domain.Enums;

    public class ProsodyScore : Entity, ITranslationObject
    {
        public EnumExamPracticeAIType AIType { get; set; }
        public EnumExamPracticeModuleAIType ModuleAIType { get; set; }

        [MaxLength(200, ErrorMessage = nameof(EnumSystemErrorCode.MaxLength))]
        public string? Language { get; set; }

        public double MinScore { get; set; }

        public double MaxScore { get; set; }

        public double BandScore { get; set; }

        public string? BandComment { get; set; }

        public int PoinsInRange
        {
            get
            {
                return (int)(MaxScore - MinScore + 1);
            }
        }
    }
}
