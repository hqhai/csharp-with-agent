// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ExtraPracticeQuery
{
    using System;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetPlacementTestByExtraPracticeQuery : IRequest<MethodResult<ExtraPracticeModel>>
    {
        public Guid ExtraPracticeId { get; set; }
        public Guid ExtraPracticeResultId { get; set; }
    }

    public class GetPlacementTestByExtraPracticeQueryHandler : IRequestHandler<GetPlacementTestByExtraPracticeQuery, MethodResult<ExtraPracticeModel>>
    {
        private readonly IExtraPracticeRepository _extraPracticeRepository;
        private readonly QuestionTypeConverter _questionTypeConverter;
        private readonly IMapper _mapper;
        private readonly IExtraPracticeResultRepository _extraPracticeResultRepository;

        public GetPlacementTestByExtraPracticeQueryHandler(IExtraPracticeRepository extraPracticeRepository
            , QuestionTypeConverter questionTypeConverter
            , IMapper mapper
            , IExtraPracticeResultRepository extraPracticeResultRepository)
        {
            _extraPracticeRepository = extraPracticeRepository;
            _questionTypeConverter = questionTypeConverter;
            _mapper = mapper;
            _extraPracticeResultRepository = extraPracticeResultRepository;
        }

        public async Task<MethodResult<ExtraPracticeModel>> Handle(GetPlacementTestByExtraPracticeQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ExtraPracticeModel> methodResult = new MethodResult<ExtraPracticeModel>();

            var extraPracticeResult = await _extraPracticeResultRepository.GetByIdAsync(request.ExtraPracticeResultId);
            if (extraPracticeResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist));
                return methodResult;
            }

            var extraPractice = await _extraPracticeRepository.Queryable.Include(x => x.ExtraPracticeResults.Where(x => !x.IsDeleted))
                                                                  .Include(x => x.PlacementTest)
                                                                  .ThenInclude(x => x!.PlacementTestSections.Where(x => !x.IsDeleted))
                                                                  .ThenInclude(x => x.SectionGroup)
                                                                  .ThenInclude(x => x!.Sections.Where(x => !x.IsDeleted))
                                                                  .ThenInclude(x => x.SectionParts.Where(x => !x.IsDeleted))
                                                                  .ThenInclude(x => x.SectionQuestions.Where(x => !x.IsDeleted))
                                                                  .ThenInclude(x => x.Question)
                                                                  .ThenInclude(x => x!.ExtraPracticeAnswers.Where(x => x.ExtraPracticeResultId == extraPracticeResult.Id))
                                                                  .AsNoTracking()
                                                                  .FirstOrDefaultAsync(x => x.Id == request.ExtraPracticeId, cancellationToken);

            var extraPracticeModel = new ExtraPracticeModel
            {
                Id = extraPractice!.Id,
                Abstract = extraPractice.Abstract,
                Author = extraPractice.Author,
                Code = extraPractice.Code,
                CourseLevel = extraPractice.CourseLevel,
                BookFilePath = extraPractice.BookFilePath,
                BookCoverPath = extraPractice.BookCoverPath,
                BookBackgroundPath = extraPractice.BookBackgroundPath,
                InstructionContent = extraPractice.InstructionContent,
                IsActive = extraPractice.IsActive,
                Name = extraPractice.Name,
                Type = extraPractice.Type,
                VideoLink = extraPractice.VideoLink,
                PlacementTest = new PlacementTestModel
                {
                    Id = extraPractice.PlacementTest!.Id,
                    Name = extraPractice.PlacementTest.Name,
                    Level = extraPractice.PlacementTest.PlacementTestLevel,
                    CreatedDate = extraPractice.PlacementTest.CreatedDate,
                    IsActive = extraPractice.PlacementTest.IsActive,
                    SectionGroups = extraPractice.PlacementTest.PlacementTestSections.Select(x => x.SectionGroup).OrderBy(x => x!.CreatedDate).Select(x => new SectionGroupModel
                    {
                        Id = x!.Id,
                        ExecutionTime = x!.ExecutionTime,
                        CourseSkill = x.CourseSkill,
                        TotalQuestion = x!.Sections.SelectMany(x => x.SectionParts).SelectMany(x => x.SectionQuestions).Select(x => x.Question).Count(),
                        Sections = x.Sections.OrderBy(x => x.DisplayOrder).Select(x => new SectionModel
                        {
                            Id = x.Id,
                            Name = x.Name,
                            MediaPost = x.MediaPost,
                            TargetWord = x.TargetWord,
                            DisplayOrder = x.DisplayOrder,
                            VideoFilePath = x.VideoFilePath,
                            SectionParts = x.SectionParts.OrderBy(x => x!.CreatedDate).Select(x => new SectionPartModel
                            {
                                Id = x.Id,
                                PartName = x.PartName,
                                SectionId = x.SectionId,
                                Questions = x.SectionQuestions.Select(x => x.Question).OrderBy(x => x!.CreatedDate).Select(m => new QuestionModel
                                {
                                    Id = m!.Id,
                                    QuestionType = m!.QuestionType,
                                    CorrectTotal = m!.CorrectTotal,
                                    Explanation = m!.Explanation,
                                    Ungraded = m!.Ungraded,
                                    Config = _questionTypeConverter.QuestionTypeConverterObject(m!.Config, m!.QuestionType, isDisableAnswers: true).Item1,
                                    ResultAnswer = _mapper.Map<AnswerModel>(m.ExtraPracticeAnswers!.FirstOrDefault())
                                }).ToList(),
                            }).ToList(),
                        }).ToList(),
                    }).ToList(),
                },
                ExtraPracticeResult = extraPractice.ExtraPracticeResults.Where(m => m.Id == extraPracticeResult.Id).Select(x => new ExtraPracticeResultModel
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
