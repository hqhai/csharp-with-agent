// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ExtraPracticeQuery
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetMockTestByExtraPracticeQuery : IRequest<MethodResult<ExtraPracticeModel>>
    {
        public Guid ExtraPracticeId { get; set; }
        public Guid ExtraPracticeResultId { get; set; }
    }

    public class GetMockTestByExtraPracticeQueryHandler : IRequestHandler<GetMockTestByExtraPracticeQuery, MethodResult<ExtraPracticeModel>>
    {
        private readonly IExtraPracticeRepository _extraPracticeRepository;
        private readonly SectionConverter _sectionConverter;
        private readonly IExtraPracticeResultRepository _extraPracticeResultRepository;

        public GetMockTestByExtraPracticeQueryHandler(IExtraPracticeRepository extraPracticeRepository, SectionConverter sectionConverter, IExtraPracticeResultRepository extraPracticeResultRepository)
        {
            _extraPracticeRepository = extraPracticeRepository;
            _sectionConverter = sectionConverter;
            _extraPracticeResultRepository = extraPracticeResultRepository;
        }

        public async Task<MethodResult<ExtraPracticeModel>> Handle(GetMockTestByExtraPracticeQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ExtraPracticeModel> methodResult = new MethodResult<ExtraPracticeModel>();

            var extraPracticeResult = await _extraPracticeResultRepository.GetByIdAsync(request.ExtraPracticeResultId);
            if (extraPracticeResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var extraPractice = await _extraPracticeRepository.Queryable
                                                                    .Include(x => x.MockTest)
                                                                          .ThenInclude(x => x!.MockTestSections)
                                                                          .ThenInclude(x => x.SectionGroup)
                                                                          .ThenInclude(x => x!.Sections.Where(x => !x.IsDeleted))
                                                                          .ThenInclude(x => x.SectionParts.Where(x => !x.IsDeleted))
                                                                          .ThenInclude(x => x.SectionQuestions.Where(x => !x.IsDeleted))
                                                                          .ThenInclude(x => x.Question)
                                                                          .ThenInclude(x => x!.ExtraPracticeAnswers.Where(x => x.ExtraPracticeResultId == extraPracticeResult!.Id))
                                                                     .Include(x => x.MockTest)
                                                                          .ThenInclude(x => x!.MockTestSections.Where(x => !x.IsDeleted))
                                                                          .ThenInclude(x => x.SectionGroup)
                                                                          .ThenInclude(x => x!.Sections.Where(x => !x.IsDeleted))
                                                                          .ThenInclude(x => x.SectionTimeCodes.Where(x => !x.IsDeleted))
                                                                          .ThenInclude(x => x.ExtraPracticeAnswers.Where(x => x.ExtraPracticeResultId == extraPracticeResult!.Id))
                                                                    .Include(x => x.MockTest)
                                                                          .ThenInclude(x => x!.MockTestSections.Where(x => !x.IsDeleted))
                                                                          .ThenInclude(x => x.SectionGroup)
                                                                          .ThenInclude(x => x!.Sections.Where(x => !x.IsDeleted))
                                                                          .ThenInclude(x => x.ExtraPracticeAnswers.Where(x => x.ExtraPracticeResultId == extraPracticeResult!.Id))
                                                                    .Include(x => x!.ExtraPracticeResults)
                                                                    .Where(x => x.Id == request.ExtraPracticeId)
                                                                          .AsNoTracking()
                                                                    .FirstOrDefaultAsync(cancellationToken);
            if (extraPractice == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }
            var extraPracticeModel = new ExtraPracticeModel
            {
                Id = extraPractice.Id,
                Abstract = extraPractice.Abstract,
                Author = extraPractice.Author,
                Code = extraPractice.Code,
                CourseLevel = extraPractice.CourseLevel,
                BookFilePath = extraPractice.BookFilePath,
                BookCoverPath = extraPractice.BookCoverPath,
                BookBackgroundPath = extraPractice.BookBackgroundPath,
                ImagePath = extraPractice.ImagePath,
                InstructionContent = extraPractice.InstructionContent,
                IsActive = extraPractice.IsActive,
                Name = extraPractice.Name,
                Type = extraPractice.Type,
                VideoLink = extraPractice.VideoLink,
                MockTest = new MockTestModel
                {
                    Id = extraPractice.MockTest!.Id,
                    Name = extraPractice.MockTest.Name,
                    MockTestType = extraPractice.MockTest.MockTestType,
                    CreatedDate = extraPractice.MockTest.CreatedDate,
                    CreatedFullName = extraPractice.MockTest.CreatedFullName,
                    CreatedUserId = extraPractice.MockTest.CreatedUserId,
                    IsActive = true,
                    ExecutionTime = extraPractice.MockTest.MockTestSections.Select(x => x.SectionGroup).Sum(x => x.ExecutionTime),
                    SectionGroups = extraPractice.MockTest.MockTestSections.Where(x => x.SectionGroup != null)
                         .Select(x => x.SectionGroup).OrderBy(x => x!.CreatedDate)
                         .Select(x => _sectionConverter.GetSectionGroupModel(x, true)).ToList(),
                },
                ExtraPracticeResult = extraPractice.ExtraPracticeResults.Where(m => m.Id == request.ExtraPracticeResultId).Select(x => new ExtraPracticeResultModel
                {
                    Id = x.Id,
                    CorrectCount = x.CorrectCount,
                    CorrectTotal = x.CorrectTotal,
                    SkillScores = x.SkillScores,
                    Percent = x.Percent,
                    Status = x.Status,
                    StudentId = x.StudentId,
                    ExtraPracticeId = x.ExtraPracticeId
                }).FirstOrDefault()
            };
            methodResult.Result = extraPracticeModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
