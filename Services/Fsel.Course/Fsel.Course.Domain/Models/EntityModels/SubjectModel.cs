// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels
{
    using Enums;

    public class SubjectModel
    {
        public Guid Id { get; set; }

        public string? Name { get; set; }

        public string? Thumbnail { get; set; }
        public string? Type { get; set; }
        public string? Description { get; set; }
        public EnumTestMode? TestMode { get; set; }

        public List<LevelModel>? Levels { get; set; } = new List<LevelModel>();

        public List<SubjectModel> ChildSubjects { get; set; } = new List<SubjectModel>();

        public bool HadLearnedBefore { get; set; }

        public bool IsCurrentLearning { get; set; }

        public bool HasLevel()
        {
            if (Levels != null && Levels.Any())
            {
                return true;
            }

            if (ChildSubjects != null && ChildSubjects.Any())
            {
                foreach (var child in ChildSubjects)
                {
                    if (child.HasLevel())
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
