// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Training.Doman.Models.CommandModels.Trainings
{
    public class CreateTrainingCommandModel
    {
        public string? Code { get; set; }
        public Guid ClassId { get; set; }
        public Guid UserId { get; set; }
    }
}
