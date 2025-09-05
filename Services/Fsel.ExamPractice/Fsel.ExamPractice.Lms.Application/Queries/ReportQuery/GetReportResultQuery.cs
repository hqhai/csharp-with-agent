// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.ExamPractice.Lms.Application.Queries.ReportQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.ExamPractice.Domain.Entities;
    using Fsel.ExamPractice.Domain.IRepositories;
    using Fsel.ExamPractice.Domain.Models.EntityModels.ExamPractices;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetReportResultQuery : IRequest<MethodResult<ExamPracticeReportViewModel>>
    {
        public Guid ExamPracticeResultId { get; set; }
    }

    public class GetReportResultQueryHandler : IRequestHandler<GetReportResultQuery, MethodResult<ExamPracticeReportViewModel>>
    {
        private readonly IExamPracticeResultRepository _examPracticeResultRepository;
        private readonly IExamPracticeSectionResultRepository _examPracticeSectionResultRepository;
        private readonly IExamPracticeSectionRepository _examPracticeSectionRepository;

        public GetReportResultQueryHandler(
            IExamPracticeResultRepository examPracticeResultRepository,
            IExamPracticeSectionResultRepository examPracticeSectionResultRepository,
            IExamPracticeSectionRepository examPracticeSectionRepository)
        {
            _examPracticeResultRepository = examPracticeResultRepository;
            _examPracticeSectionResultRepository = examPracticeSectionResultRepository;
            _examPracticeSectionRepository = examPracticeSectionRepository;
        }

        public async Task<MethodResult<ExamPracticeReportViewModel>> Handle(GetReportResultQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ExamPracticeReportViewModel>();
            var examPracticeResult = await _examPracticeResultRepository.Queryable
                                                                        .Where(epr => epr.Id == request.ExamPracticeResultId)
                                                                        .Select(epr => new
                                                                        {
                                                                            epr.Id,
                                                                            ExamName = epr.ExamPractice != null ? epr.ExamPractice.Name : null
                                                                        })
                                                                        .AsNoTracking()
                                                                        .FirstOrDefaultAsync(cancellationToken);
            if (examPracticeResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(examPracticeResult), request.ExamPracticeResultId);
                return methodResult;
            }

            var examPracticeSectionResults = await _examPracticeSectionResultRepository.Queryable
                                                              .Where(epsr => epsr.ExamPracticeResultId == request.ExamPracticeResultId)
                                                              .Where(x => x.ParentExamPracticeSectionResultId == null)
                                                              .Select(sr => new ExamPracticeSectionResult
                                                              {
                                                                  Id = sr.Id,
                                                                  ExamPracticeSectionId = sr.ExamPracticeSectionId,
                                                                  ParentExamPracticeSectionResultId = sr.ParentExamPracticeSectionResultId,
                                                                  SkillScores = sr.SkillScores
                                                              })
                                                              .AsNoTracking()
                                                              .ToListAsync(cancellationToken);

            var childrenResults = await (from baseQ in _examPracticeSectionResultRepository.Queryable.Where(epsr => epsr.ExamPracticeResultId == request.ExamPracticeResultId)
                                                                                                     .Where(x => x.ParentExamPracticeSectionResultId.HasValue)
                                         join es in _examPracticeSectionRepository.Queryable on baseQ.ExamPracticeSectionId equals es.Id
                                         select new
                                         {
                                             ExamPracticeSectionResult = new ExamPracticeSectionResult
                                             {
                                                 SkillScores = baseQ.SkillScores,
                                                 ExamPracticeSectionId = baseQ.ExamPracticeSectionId,
                                                 ParentExamPracticeSectionResultId = baseQ.ParentExamPracticeSectionResultId
                                             },
                                             es.DisplayOrder
                                         })
                                         .AsNoTracking()
                                         .ToListAsync(cancellationToken);
            var childrenLookup = childrenResults.Where(s => s.ExamPracticeSectionResult.ParentExamPracticeSectionResultId.HasValue)
                                                .GroupBy(s => s.ExamPracticeSectionResult.ParentExamPracticeSectionResultId!.Value)
                                                .ToDictionary(g => g.Key, g => g.OrderBy(x => x.DisplayOrder).Select(x => x.ExamPracticeSectionResult).ToList());

            ExamPracticeReportViewModel examPracticeReportView = new ExamPracticeReportViewModel();
            examPracticeReportView.Name = examPracticeResult.ExamName;
            examPracticeReportView.ExamPracticeResultId = examPracticeResult.Id;
            foreach (var item in examPracticeSectionResults)
            {
                childrenLookup.TryGetValue(item.Id, out var list);
                var section = new ExamPracticeReportSkillViewModel
                {
                    ExamPracticeSectionId = item.ExamPracticeSectionId,
                    SkillScores = item.SkillScores,
                    Childrens = list?.Select(child => new ExamPracticeReportSkillViewModel
                    {
                        ExamPracticeSectionId = child.ExamPracticeSectionId,
                        SkillScores = child.SkillScores
                    }).ToList()
                };
                examPracticeReportView.Sections.Add(section);
            }
            methodResult.Result = examPracticeReportView;
            return methodResult;
        }
    }
}
