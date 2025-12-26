// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.PlacementTestQuery
{
    using Common.ActionResults;
    using Core.Base.Interfaces;
    using Domain.Entities.TestConfigs;
    using Domain.Models.EntityModels;
    using Domain.Models.EntityModels.PlacementTestModels;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetSectionResultDetailQuery : IRequest<MethodResult<SectionStateModel>>
    {
        public Guid SectionResultId { get; set; }
    }

    public class GetSectionResultDetailQueryHandler : IRequestHandler<GetSectionResultDetailQuery, MethodResult<SectionStateModel>>
    {
        private readonly IRepository<TestSectionResult> _testSectionResultRepository;

        public GetSectionResultDetailQueryHandler(IRepository<TestSectionResult> testSectionResultRepository)
        {
            _testSectionResultRepository = testSectionResultRepository;
        }

        public async Task<MethodResult<SectionStateModel>> Handle(GetSectionResultDetailQuery request, CancellationToken cancellationToken)
        {
            var testSectionResult = await _testSectionResultRepository.ReadQueryable.Where(x => x.Id == request.SectionResultId)
                .Include(x => x.TestSection)
                .ThenInclude(x => x.TestSectionQuestions)
                .Include(x => x.TestAnswers)
                .FirstOrDefaultAsync(cancellationToken);

            var sectionStateModel = new SectionStateModel { SectionResultId = request.SectionResultId };

            if (testSectionResult == null)
            {
                return new MethodResult<SectionStateModel>(sectionStateModel);
            }

            sectionStateModel.CorrectCount = testSectionResult.CorrectCount;
            sectionStateModel.TotalCount = testSectionResult.CorrectTotal;
            sectionStateModel.Config = testSectionResult.TestSection.Config;

            if (testSectionResult.TestSection?.TestSectionQuestions != null)
            {
                sectionStateModel.Children = testSectionResult.TestSection.TestSectionQuestions.OrderBy(x => x.CreatedDate).Select(BaseTestStateModel (x) =>
                {
                    var questionModel = new QuestionStateModel { QuestionId = x.QuestionId };
                    var testAnswer = testSectionResult?.TestAnswers.FirstOrDefault(t => t.QuestionId == x.QuestionId);
                    questionModel.TestAnswerId = testAnswer?.Id;
                    if (testAnswer != null)
                    {
                        questionModel.Answer = new AnswerModel { Answer = testAnswer.Answer, CorrectCount = testAnswer.CorrectCount, IsCorrect = testAnswer.IsCorrect, };
                    }

                    return questionModel;
                }).ToList();
            }

            return new MethodResult<SectionStateModel>(sectionStateModel);
        }
    }
}
