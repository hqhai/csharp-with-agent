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
    }
}
