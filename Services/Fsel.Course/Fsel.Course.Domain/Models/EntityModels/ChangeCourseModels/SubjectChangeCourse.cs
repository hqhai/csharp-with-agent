// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.ChangeCourseModels
{
    using Enums;
    using Entities;

    public class SubjectChangeCourse : ChangeCourseComposite
    {
        public override ChangeCourseDirective? ChangeCourse(ChangeCourseRequest request)
        {
            foreach (var child in Children)
            {
                var changeCourseResponse = child.ChangeCourse(request);
                if (changeCourseResponse != null)
                {
                    if (changeCourseResponse.Action == EnumChangeCourseAction.ChangeAndStartPt && child is ProgramChangeCourse { TestMode: EnumTestMode.Default })
                    {
                        if (Children.FirstOrDefault(x => x is ProgramChangeCourse { TestMode: EnumTestMode.Custom, IsDefaultForTest: true }) is ProgramChangeCourse
                            programHadPtDefault)
                        {
                            changeCourseResponse.ProgramOwnPt = programHadPtDefault.Id;
                        }
                    }

                    return changeCourseResponse;
                }
            }

            return null;
        }

        public override ChangeProgramDirective? ChangeProgram(ChangeProgramRequest request)
        {
            foreach (var child in Children.OfType<ChangeCourseComposite>())
            {
                var changeProgramResponse = child.ChangeProgram(request);
                if (changeProgramResponse != null)
                {
                    if (changeProgramResponse.Action == EnumChangeProgramAction.ChangeAndStartPt && child is ProgramChangeCourse { TestMode: EnumTestMode.Default })
                    {
                        if (Children.FirstOrDefault(x => x is ProgramChangeCourse { TestMode: EnumTestMode.Custom, IsDefaultForTest: true }) is ProgramChangeCourse
                            programCustomHadPtResult)
                        {
                            changeProgramResponse.ProgramOwnPt = programCustomHadPtResult.Id;
                        }
                    }

                    return changeProgramResponse;
                }
            }

            return null;
        }

        public override ChangeSubjectDirective? ChangeSubject(ChangeProgramRequest request)
        {
            if (IsProject())
            {
                return base.ChangeSubject(request);
            }
            else
            {
                var childDirectives = Children
                 .Select(child => child.ChangeSubject(request))
                 .OfType<ChangeSubjectDirective>()
                 .OrderByDescending(x => x.CreatedOrUpdatedDate)
                 .ToList();

                if (childDirectives.Count(x => x.Action == EnumChangeSubjectAction.ChangeToRecentCourse) < Children.Count)
                {
                    return new ChangeSubjectDirective
                    {
                        Action = EnumChangeSubjectAction.ChangeAndStartPt,
                    };
                }
                else
                {
                    return childDirectives.FirstOrDefault();
                }
            }
        }

        public override ChangeSubjectDirective? SelectProjectSubject(ChangeProgramRequest request)
        {
            if (IsProject())
            {
                if (Id != request.ProgramId)
                {
                    return null;
                }

                return Children
                .Select(child => child.ChangeSubject(request))
                .OfType<ChangeSubjectDirective>()
                .OrderByDescending(x => x.CreatedOrUpdatedDate)
                .FirstOrDefault();
            }
            else
            {
                foreach (var child in Children)
                {
                    var changeSubjectResponse = child.SelectProjectSubject(request);
                    if (changeSubjectResponse != null)
                    {
                        return changeSubjectResponse;
                    }
                }

                return null;
            }
        }

        private bool IsProject()
        {
            return Children.All(c => c is ProgramChangeCourse);
        }
    }
}
