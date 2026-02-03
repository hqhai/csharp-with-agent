// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.ChangeCourseModels
{
    using System;
    using System.Collections.Generic;
    using Entities;

    public abstract class ChangeCourseComposite : ChangeCourseComponent
    {
        public Category Category { get; set; }

        public List<ChangeCourseComponent> Children { get; set; } = new List<ChangeCourseComponent>();

        public Guid Id { get; set; }

        public override IEnumerable<T> GetComponentsByType<T>()
        {
            if (this is T component)
            {
                yield return component;
            }

            foreach (var childComponent in Children.SelectMany(child => child.GetComponentsByType<T>()))
            {
                yield return childComponent;
            }
        }

        public override bool Contain(Guid targetLevelId)
        {
            if (Id == targetLevelId)
            {
                return true;
            }

            return Children?.FirstOrDefault(child => child.Contain(targetLevelId)) != null;
        }

        public abstract ChangeProgramDirective? ChangeProgram(ChangeProgramRequest request);

        public override ChangeSubjectDirective? ChangeSubject(ChangeProgramRequest request)
        {
            return Children
                .Select(child => child.ChangeSubject(request))
                .OfType<ChangeSubjectDirective>()
                .OrderByDescending(x => x.CreatedOrUpdatedDate)
                .FirstOrDefault();
        }

        public override bool IsCurrentLearning()
        {
            return Children.Any(child => child.IsCurrentLearning());
        }

        public virtual SubjectModel GetSubjectTree(bool isIncludeLevel = false)
        {
            var subjectModel = new SubjectModel
            {
                Id = Id,
                Name = Category.Name,
                Thumbnail = Category.Thumbnail,
                Type = Category.Type.ToString(),
                TestMode = Category.TestMode,
                Description = Category?.Description,
                IsCurrentLearning = IsCurrentLearning(),
                ChildSubjects = Children
                    .OfType<ChangeCourseComposite>()
                    .Select(child => child.GetSubjectTree(isIncludeLevel))
                    .ToList(),
                Levels = Children
                    .OfType<LevelChangeCourse>()
                    .Select(child => new SelectionLevelModel
                    {
                        Id = child.LevelId,
                        Name = child.Name,
                        Code = child.Level.Code,
                        Description = child.Level.Description,
                        LevelOrder = child.Level.LevelOrder,
                        LearnedBefore = child.LearnedBefore,
                        CanSelect = child.CanSelect,
                        ProgramId = child.Level.ProgramId,
                        IsCurrentLevel = child.IsCurrentLearningLevel,
                    } as LevelModel)
                    .ToList()
            };

            subjectModel.HadLearnedBefore = subjectModel.ChildSubjects.Any(c => c.HadLearnedBefore) || subjectModel.Levels.Any(l => l.LearnedBefore);
            if (Children.Any(c => c is ProgramChangeCourse or LevelChangeCourse))
            {
                if (!isIncludeLevel)
                {
                    subjectModel.Levels = new List<LevelModel>();
                }
            }

            return subjectModel;
        }

        public override FromInfo? GetCurrentInfo()
        {
            foreach (var chil in Children)
            {
                var info = chil.GetCurrentInfo();
                if (info != null)
                {
                    return info;
                }
            }

            return null;
        }
    }
}
