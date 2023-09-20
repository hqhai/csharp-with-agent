// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Application.Queries.ExtraPracticeQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetExtraPracticeSkillByIdQuery : IRequest<MethodResult<ExtraPracticeSkillModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetExtraPracticeSkillByIdQueryHandler : IRequestHandler<GetExtraPracticeSkillByIdQuery, MethodResult<ExtraPracticeSkillModel>>
    {
        private readonly IExtraPracticeRepository _extraPracticeRepository;

        public GetExtraPracticeSkillByIdQueryHandler(IExtraPracticeRepository extraPracticeRepository)
        {
            _extraPracticeRepository = extraPracticeRepository;
        }

        public async Task<MethodResult<ExtraPracticeSkillModel>> Handle(GetExtraPracticeSkillByIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<ExtraPracticeSkillModel> methodResult = new MethodResult<ExtraPracticeSkillModel>();
            var extraPractice = await _extraPracticeRepository.Queryable
                        .Include(x => x.PlacementTest)
                            .ThenInclude(x => x!.PlacementTestSections)
                            .ThenInclude(x => x.SectionGroup)
                        .Include(x => x.MockTest)
                            .ThenInclude(x => x!.MockTestSections)
                            .ThenInclude(x => x.SectionGroup)
                        .Include(x => x.ExtraPracticeChapters)
                            .ThenInclude(x => x.ExtraPracticeExercises)
                                .ThenInclude(x => x.Exercise)
                        .Include(x => x.ExtraPracticeExercises)
                            .ThenInclude(x => x.Exercise)
                        .Include(x => x.Video)
                            .ThenInclude(x => x!.VideoTimeCodes)
                                .ThenInclude(x => x.TimeCodeExercises)
                                    .ThenInclude(x => x.Exercise)
                        .Where(x => x.IsActive && x.Id == request.Id)
                        .AsNoTracking()
                        .Select(x => new ExtraPracticeSkillModel
                        {
                            Id = x.Id,
                            Code = x.Code,
                            Name = x.Name,
                            IsActive = x.IsActive,
                            InstructionContent = x.InstructionContent,
                            CreatedFullName = x.CreatedFullName,
                            CreatedUserId = x.CreatedUserId,
                            UpdatedDate = x.UpdatedDate,
                            UpdatedFullName = x.UpdatedFullName,
                            UpdatedUserId = x.UpdatedUserId,
                            CreatedDate = x.CreatedDate,
                            Type = x.Type,
                            CourseLevel = x.CourseLevel,
                            CourseSkills = GetCourseSkills(x),
                        }).FirstOrDefaultAsync(cancellationToken);

            if (extraPractice == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(extraPractice));
                return methodResult;
            }

            methodResult.Result = extraPractice;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private static IList<EnumCourseSkill>? GetCourseSkills(ExtraPractice extraPractice)
        {
            ArgumentNullException.ThrowIfNull(extraPractice);
            var courseSkills = new List<EnumCourseSkill>();
            switch (extraPractice.Type)
            {
                case EnumExtraPracticeType.Book:
                    courseSkills = extraPractice.ExtraPracticeChapters.SelectMany(x => x.ExtraPracticeExercises).Select(x => x.Exercise).Distinct().Select(x => x!.CourseSkill).ToList();
                    break;

                case EnumExtraPracticeType.VideoEmbed:
                    courseSkills = extraPractice.ExtraPracticeExercises.Select(x => x.Exercise).Select(x => x!.CourseSkill).Distinct().ToList();
                    break;

                case EnumExtraPracticeType.InteractiveVideo:
                    courseSkills = extraPractice.Video != null ? extraPractice.Video.VideoTimeCodes.SelectMany(x => x!.TimeCodeExercises).Select(x => x.Exercise).Select(x => x!.CourseSkill).Distinct().ToList() : null;
                    break;

                case EnumExtraPracticeType.MockTest:
                    if (extraPractice.MockTestId != null)
                    {
                        courseSkills = extraPractice.MockTest?.MockTestSections.Select(x => x.SectionGroup).Select(x => x!.CourseSkill).Distinct().ToList();
                    }
                    else
                    {
                        courseSkills = extraPractice.PlacementTest?.PlacementTestSections.Select(x => x.SectionGroup).Select(x => x!.CourseSkill).Distinct().ToList();
                    }
                    break;

                case EnumExtraPracticeType.Exercise:
                    courseSkills = extraPractice.ExtraPracticeExercises.Select(x => x.Exercise).Select(x => x!.CourseSkill).Distinct().ToList();
                    break;

                case EnumExtraPracticeType.Articles:
                    courseSkills = null;
                    break;
            }
            return courseSkills;
        }
    }
}
