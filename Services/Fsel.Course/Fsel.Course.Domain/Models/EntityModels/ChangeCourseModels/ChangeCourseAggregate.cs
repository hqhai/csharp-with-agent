// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.ChangeCourseModels
{
    public class ChangeCourseAggregate
    {
        public ChangeCourseAggregate(ICollection<SubjectChangeCourse> subjects)
        {
            RootSubjects = subjects;
        }

        public ICollection<SubjectChangeCourse> RootSubjects { get; set; }

        public ChangeCourseDirective? ChangeCourse(ChangeCourseRequest request)
        {
            foreach (var subject in RootSubjects)
            {
                var directive = subject.ChangeCourse(request);
                if (directive != null)
                {
                    directive.FromInfo = subject.GetCurrentInfo();
                    return directive;
                }
            }

            return null;
        }

        public ChangeProgramDirective? ChangeProgram(ChangeProgramRequest request)
        {
            return RootSubjects.Select(subject => subject.ChangeProgram(request)).OfType<ChangeProgramDirective>().FirstOrDefault();
        }
    }
}
