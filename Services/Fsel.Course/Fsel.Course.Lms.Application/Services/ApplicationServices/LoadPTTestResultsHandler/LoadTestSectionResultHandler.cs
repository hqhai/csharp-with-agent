// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.LoadPTTestResultsHandler
{
    using System.Threading.Tasks;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.Models.EntityModels.PlacementTestModels;

    public interface ILoadTestSectionResultHandler : IBaseLoadPTTestResultHandler
    {
    }

    public class LoadTestSectionResultHandler : BaseLoadPTTestResultHandler, ILoadTestSectionResultHandler
    {
        private readonly ITestService _testService;

        public LoadTestSectionResultHandler(ITestService testService)
        {
            _testService = testService;
        }

        public override async Task Handle(LoadPTTestResultContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            var inprogressModule = context.PTState.Modules?.FirstOrDefault(x => x.Status == Domain.Enums.EnumResultStatus.Process);
            if (inprogressModule == null)
            {
                return;
            }

            var testResult = await _testService.LoadHierachicalTestResult(x => x.Id == inprogressModule.TestResultId, isReadOnly: true);

            inprogressModule.Skills = testResult.SectionResults.Select(x => Create(x)).ToList();

            if (Next != null)
            {
                await Next.Handle(context);
            }
        }

        public static SectionStateModel Create(TestSectionResult testSectionResult)
        {
            var sectionState = new SectionStateModel
            {
                SectionId = testSectionResult.TestSectionId,
                SectionResultId = testSectionResult.Id,
                Status = testSectionResult.Status,
                Questions = testSectionResult.TestAnswers?.Select(ta => new QuestionStateModel
                {
                    QuestionId = ta.QuestionId,
                    QuestionResultId = ta.Id,
                    Status = ta.Status
                }).ToList() ?? new List<QuestionStateModel>(),
                ChildSections = testSectionResult.SectionResults?.Select(sr => Create(sr)).ToList() ?? new List<SectionStateModel>()
            };

            return sectionState;
        }
    }
}
