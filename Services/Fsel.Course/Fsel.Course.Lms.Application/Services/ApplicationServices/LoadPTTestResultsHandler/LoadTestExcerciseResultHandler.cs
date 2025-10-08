// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.LoadPTTestResultsHandler
{
    using System.Threading.Tasks;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.Models.EntityModels.PlacementTestModels;
    using Microsoft.EntityFrameworkCore;

    public interface ILoadTestExcerciseResultHandler : IBaseLoadPTTestResultHandler
    {
    }

    public class LoadTestExcerciseResultHandler : BaseLoadPTTestResultHandler, ILoadTestExcerciseResultHandler
    {
        private readonly IRepository<TestSectionResult> _excerciseResultRepository;

        public LoadTestExcerciseResultHandler(IRepository<TestSectionResult> excerciseResultRepository)
        {
            _excerciseResultRepository = excerciseResultRepository;
        }

        public override async Task Handle(LoadPTTestResultContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            var inprogressModule = context.PTState.Modules?.FirstOrDefault(x => x.Status == Domain.Enums.EnumResultStatus.Process);
            if (inprogressModule == null)
            {
                return;
            }
            var inprogressSkill = inprogressModule.Skills?.FirstOrDefault(x => x.Status == Domain.Enums.EnumResultStatus.Process);
            if (inprogressSkill == null)
            {
                return;
            }

            var excerciseResults = await _excerciseResultRepository.ReadQueryable
                 .Where(x => x.ParentTestSectionResultId == inprogressSkill.SectionResultId)
                 .ToListAsync();

            inprogressSkill.Exercises = excerciseResults.Select(x => new ExcerciseStateModel
            {
                SectionId = x.TestSectionId,
                SectionResultId = x.Id,
                Status = x.Status
            }).ToList();

            if (Next != null)
            {
                await Next.Handle(context);
            }
        }
    }
}
