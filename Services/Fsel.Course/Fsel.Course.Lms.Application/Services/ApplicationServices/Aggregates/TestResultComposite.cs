// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.Aggregates
{
    using System.Threading.Tasks;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.Enums;

    public class TestResultComposite : ResultComposite
    {
        public TestResult TestResult => (TestResult)Result;

        public override async Task Submit()
        {
            await base.Submit();

            if (Children.All(c => c is TestSectionResultComposite tcr && tcr.TestSectionResult.Status == EnumResultStatus.Done))
            {
                TestResult.Status = EnumResultStatus.Done;
            }
        }

        public override void Start()
        {
            if (TestResult.Status == EnumResultStatus.New)
            {
                TestResult.Status = EnumResultStatus.Process;
            }

            if (Children != null && Children.Count > 0)
            {
                var childCanStart = Children.FirstOrDefault(x => x is TestSectionResultComposite tsr
                && (tsr.TestSectionResult.Status == EnumResultStatus.New || tsr.TestSectionResult.Status == EnumResultStatus.Unfinished)) as TestSectionResultComposite;

                childCanStart?.Start();
            }
        }

        public override void GenerateChildren()
        {
            if (TestResult.SectionResults != null && TestResult.SectionResults.Any())
            {
                foreach (var sectionResult in TestResult.SectionResults)
                {
                    var sectionComposite = new TestSectionResultComposite { Result = sectionResult, Parent = this };
                    Children.Add(sectionComposite);
                    sectionComposite.GenerateChildren();
                }
            }
        }

    }
}
