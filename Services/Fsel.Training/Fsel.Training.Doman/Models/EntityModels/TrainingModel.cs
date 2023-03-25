// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Doman.Models.EntityModels
{
    using Fsel.Training.Doman.Enums;
    using Fsel.Core.Base.BaseModels;

    public class TrainingModel : BaseModel
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public DateTime TimeStart { get; set; }

        public DateTime TimeEnd { get; set; }

        public EnumTrainingType Status { get; set; }

        public Guid StudentId { get; set; }
    }
}
