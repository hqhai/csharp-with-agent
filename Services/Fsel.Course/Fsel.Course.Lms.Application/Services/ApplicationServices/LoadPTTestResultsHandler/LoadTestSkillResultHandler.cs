// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.LoadPTTestResultsHandler
{
    using System.Threading.Tasks;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.Models.EntityModels.PlacementTestModels;
    using Microsoft.EntityFrameworkCore;

    public interface ILoadTestSkillResultHandler : IBaseLoadPTTestResultHandler
    {
    }

    public class LoadTestSkillResultHandler : BaseLoadPTTestResultHandler, ILoadTestSkillResultHandler
    {
        private readonly IRepository<TestSectionResult> _skillResultRepository;

        public LoadTestSkillResultHandler(IRepository<TestSectionResult> skillResultRepository)
        {
            _skillResultRepository = skillResultRepository;
        }

        public override async Task Handle(LoadPTTestResultContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            var inprogressModule = context.PTState.Modules?.FirstOrDefault(x => x.Status == Domain.Enums.EnumResultStatus.Process);
            if (inprogressModule == null)
            {
                return;
            }
            var skillResults = await _skillResultRepository.ReadQueryable
                 .Where(x => x.TestResultId == inprogressModule.TestResultId)
                 .ToListAsync();

            inprogressModule.Skills = skillResults.Select(x => new SkillStateModel
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
