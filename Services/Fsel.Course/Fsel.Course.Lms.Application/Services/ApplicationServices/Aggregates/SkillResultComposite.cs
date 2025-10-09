// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.Aggregates
{
    public class SkillResultComposite : TestSectionResultComposite
    {
        public override async Task Submit()
        {
            await base.Submit();
        }
    }
}
