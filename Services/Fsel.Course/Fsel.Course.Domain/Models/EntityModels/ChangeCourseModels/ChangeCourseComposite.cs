// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.ChangeCourseModels
{
    using System;
    using System.Collections.Generic;
    using Entities;

    public abstract class ChangeCourseComposite : ChangeCourseComponent
    {
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
