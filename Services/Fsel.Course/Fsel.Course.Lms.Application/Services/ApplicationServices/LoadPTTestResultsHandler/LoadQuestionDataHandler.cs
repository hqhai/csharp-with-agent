// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Services.ApplicationServices.LoadPTTestResultsHandler
{
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Core.Base.Interfaces;
    using Fsel.Course.Domain.Entities.TestConfigs;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Domain.Models.EntityModels.PlacementTestModels;
    using Microsoft.EntityFrameworkCore;

    public interface ILoadQuestionDataHandler : IBaseLoadPTTestResultHandler
    {
    }

    public class LoadQuestionDataHandler : BaseLoadPTTestResultHandler, ILoadQuestionDataHandler
    {
        private readonly ITestSectionQuestionRepository _testSectionQuestionRepository;
        private readonly IRepository<TestAnswer> _testAnswerRepository;
        private readonly IMapper _mapper;

        public LoadQuestionDataHandler(ITestSectionQuestionRepository testSectionQuestionRepository,
            IRepository<TestAnswer> testAnswerRepository,
            IMapper mapper)
        {
            _testSectionQuestionRepository = testSectionQuestionRepository;
            _testAnswerRepository = testAnswerRepository;
            _mapper = mapper;
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

            foreach (var excercise in inprogressSkill.Exercises)
            {
                var questions = await _testSectionQuestionRepository.ReadQueryable.Where(x => x.TestSectionId == excercise.SectionId)
                    .Include(x => x.Question)
                    .ToListAsync();

                var answers = await _testAnswerRepository.ReadQueryable.Where(x => x.TestSectionResultId == excercise.SectionResultId).ToListAsync();

                excercise.Questions = questions.ConvertAll(x =>
                {
                    var answer = answers.Find(a => a.QuestionId == x.QuestionId);
                    return new QuestionStateModel
                    {
                        QuestionId = x.QuestionId,
                        QuestionResultId = answer?.Id,
                        Question = _mapper.Map<QuestionModel>(x.Question),
                        Answer = answer == null ? null : _mapper.Map<AnswerModel>(answer),
                        Status = answer == null ? Shared.Enums.EnumAnswerStatus.Process : answer.Status
                    };
                });
            }

            if (Next != null)
            {
                await Next.Handle(context);
            }
        }
    }
}
