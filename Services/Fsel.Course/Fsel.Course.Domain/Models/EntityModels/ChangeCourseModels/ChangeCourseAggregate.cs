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

        public ChangeSubjectDirective? ChangeSubject(ChangeProgramRequest request)
        {
            var subject = RootSubjects.FirstOrDefault(s => s.Id == request.ProgramId);

            return subject?.ChangeSubject(request);
        }

        public Guid? GetCurrentLearningSubject()
        {
            return RootSubjects.FirstOrDefault(subject => subject.IsCurrentLearning())?.Id;
        }

        public ChangeSubjectDirective? SelectProjectSubject(ChangeProgramRequest request)
        {
            foreach (var subject in RootSubjects)
            {
                var directive = subject.SelectProjectSubject(request);
                if (directive != null)
                {
                    return directive;
                }
            }

            return null;
        }

        public List<SubjectModel> GetSubjectTree(bool isIncludeLevel = false)
        {
            return RootSubjects.Select(s => s.GetSubjectTree(isIncludeLevel)).ToList();
        }
    }
}
