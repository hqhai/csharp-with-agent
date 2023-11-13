// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Domain.Models.CommandModels.QuestBoards
{
    using Fsel.Shared.Enums;

    public class CreateQuestBoardStudentModel
    {
        public Guid StudentId { get; set; }

        public IList<EnumQuestBoardCategory>? Categories { get; set; }

        public float AchievedPoints { get; set; }

        public Guid? ObjectId { get; set; }

        public Guid CourseId { get; set; }

        public DateTime? ImplementTime { get; set; }
    }
}
