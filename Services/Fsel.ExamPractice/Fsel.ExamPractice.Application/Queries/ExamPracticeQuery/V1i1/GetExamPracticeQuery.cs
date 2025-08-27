using AutoMapper;
using Fsel.Common.ActionResults;
using Fsel.Common.Enums;
using Fsel.Common.Enums.ErrorCodes;
using Fsel.ExamPractice.Domain.Entities;
using Fsel.ExamPractice.Domain.IRepositories;
using Fsel.ExamPractice.Domain.Models.EntityModels;
using Fsel.ExamPractice.Domain.Models.EntityModels.ExamPractices;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Fsel.ExamPractice.Application.Queries.ExamPracticeQuery.V1i1
{
    public class GetExamPracticeQuery : IRequest<MethodResult<ExamPracticeModel>>
    {
        public Guid OriginalId { get; set; }
    }

    public class GetExamPracticeQueryHandler : IRequestHandler<GetExamPracticeQuery, MethodResult<ExamPracticeModel>>
    {
        private readonly IExamPracticeRepository _examPracticeRepository;
        private readonly IMapper _mapper;
        private readonly IExamPracticeSectionRepository _examPracticeSectionRepository;

        public GetExamPracticeQueryHandler(IExamPracticeRepository examPracticeRepository,
            IMapper mapper,
            IExamPracticeSectionRepository examPracticeSectionRepository)
        {
            _examPracticeRepository = examPracticeRepository;
            _mapper = mapper;
            _examPracticeSectionRepository = examPracticeSectionRepository;
        }

        public async Task<MethodResult<ExamPracticeModel>> Handle(GetExamPracticeQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ExamPracticeModel>();
            var examPractice = await _examPracticeRepository.Queryable.Where(x => x.OriginalId == request.OriginalId && x.VersionStatus == EnumVersionStatus.LastVersion && !x.IsArchive).Select(x => new
            {
                ExamPractice = x,
                ExamPracticeSections = x.ExamPracticeSections.OrderBy(x => x.DisplayOrder).ToList(),
                TotalAttempts = x.ExamPracticeResults.Count()
            }).FirstOrDefaultAsync(cancellationToken);
            if (examPractice == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(request.OriginalId), request.OriginalId);
                return methodResult;
            }
            var examPracticeSections = await GetExamPracticeSectionsBranchToLeafAsync(examPractice.ExamPracticeSections);
            var examPracticeDetail = _mapper.Map<ExamPracticeModel>(examPractice.ExamPractice);
            examPracticeDetail.TotalAttempts = examPractice.TotalAttempts;
            examPracticeDetail.ExamPracticeSections = _mapper.Map<IList<ExamPracticeSectionModel>>(examPracticeSections);
            methodResult.Result = examPracticeDetail;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<List<ExamPracticeSection>> GetExamPracticeSectionsBranchToLeafAsync(List<ExamPracticeSection> sections)
        {
            var result = new List<ExamPracticeSection>();

            foreach (var section in sections)
            {
                var subSections = await _examPracticeSectionRepository.Queryable.Include(x => x.Questions)
                    .Include(x => x.ExamPracticeAISettings)
                    .ThenInclude(x => x.ExamPracticeAICriteriaSettings)
                    .Where(x => x.ParentExamPracticeSectionId == section.Id)
                    .OrderBy(x => x.DisplayOrder)
                    .ToListAsync();
                foreach (var sub in subSections)
                {
                    sub.Questions = sub.Questions.OrderBy(q => q.CreatedDate).ToList();
                    sub.ExamPracticeAISettings = sub.ExamPracticeAISettings.OrderBy(ai => ai.CreatedDate).ToList();
                }
                if (subSections.Any() && section.ExamPracticeId.HasValue)
                {
                    result.Add(section);

                    var childResults = await GetExamPracticeSectionsBranchToLeafAsync(subSections);
                    result.AddRange(childResults);
                }
            }

            if (!result.Any() && sections.Any() && sections.All(x => x.ExamPracticeId.HasValue))
            {
                result = await _examPracticeSectionRepository.Queryable.Include(x => x.Questions)
                    .Include(x => x.ExamPracticeAISettings)
                    .ThenInclude(x => x.ExamPracticeAICriteriaSettings)
                    .WhereBulkContains(sections.Select(x => x.Id), x => x.Id)
                    .OrderBy(x => x.DisplayOrder)
                    .ToListAsync();
                foreach (var sub in result)
                {
                    sub.Questions = sub.Questions.OrderBy(q => q.CreatedDate).ToList();
                    sub.ExamPracticeAISettings = sub.ExamPracticeAISettings.OrderBy(ai => ai.CreatedDate).ToList();
                }
            }
            return result;
        }
    }
}
