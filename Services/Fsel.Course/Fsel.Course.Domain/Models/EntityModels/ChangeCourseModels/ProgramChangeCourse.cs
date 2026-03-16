// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Domain.Models.EntityModels.ChangeCourseModels
{
    using Enums;
    using Fsel.Course.Domain.Entities;

    public class ProgramChangeCourse : ChangeCourseComposite
    {
        public bool HadPtResult => PtResultId.HasValue;
        public Guid? PtResultId { get; set; }
        public EnumTestMode TestMode { get; set; }
        public bool IsDefaultForTest { get; set; }

        public override ChangeCourseDirective? ChangeCourse(ChangeCourseRequest request)
        {
            foreach (var changeCourseResponse in Children.Select(child => child.ChangeCourse(request)).OfType<ChangeCourseDirective>())
            {
                changeCourseResponse.ToProgramId = Id;
                if (changeCourseResponse.Action == EnumChangeCourseAction.ChangeDirectly)
                {
                    if (TestMode == EnumTestMode.Not && !HadPtResult)
                    {
                        changeCourseResponse.Action = EnumChangeCourseAction.ChangeDirectlyBecauseByPass;
                    }
                    changeCourseResponse.PtResultId = PtResultId;
                }
                else if (changeCourseResponse.Action == EnumChangeCourseAction.ChangeAndStartPt)
                {
                    if (TestMode == EnumTestMode.Custom)
                    {
                        changeCourseResponse.ProgramOwnPt = Id;
                    }
                }

                return changeCourseResponse;
            }

            return null;
        }

        public override ChangeProgramDirective? ChangeProgram(ChangeProgramRequest request)
        {
            if (request?.ProgramId == Id)
            {
                if (HadPtResult)
                {
                    return new ChangeProgramDirective
                    {
                        ToProgramId = Id,
                        RelatedPtResultId = PtResultId,
                        Action = EnumChangeProgramAction.ChangeToProgramExistedPt
                    };
                }
                else if (TestMode == EnumTestMode.Not)
                {
                    return new ChangeProgramDirective
                    {
                        ToProgramId = Id,
                        Action = EnumChangeProgramAction.ChangeDirectlyBecauseByPass
                    };
                }

                return new ChangeProgramDirective
                {
                    ToProgramId = Id,
                    ProgramOwnPt = TestMode == EnumTestMode.Custom ? Id : null,
                    Action = EnumChangeProgramAction.ChangeAndStartPt
                };
            }

            return null;
        }

        public override FromInfo? GetCurrentInfo()
        {
            foreach (var info in Children.Select(chil => chil.GetCurrentInfo()).OfType<FromInfo>())
            {
                info.ProgramId = Id;
                return info;
            }

            return null;
        }

        public override ChangeSubjectDirective? SelectProjectSubject(ChangeProgramRequest request)
        {
            return base.ChangeSubject(request);
        }
    }
}
