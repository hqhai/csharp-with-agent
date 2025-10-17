// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.PlacementTestQuery
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Constants;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetPlacementTestQuery : IRequest<MethodResult<PlacementTestBankModel>>
    {
        public EnumPlacementTestLevel Level { get; set; }
    }

    public class GetPlacementTestQueryHandler : IRequestHandler<GetPlacementTestQuery, MethodResult<PlacementTestBankModel>>
    {
        private readonly AuthContext _authContext;
        private readonly SectionGroupConverter _sectionGroupConverter;
        private readonly IPlacementTestResultRepository _placementTestResultRepository;
        private readonly IUserService _userService;
        private readonly IPlacementTestRepository _placementTestRepository;

        public GetPlacementTestQueryHandler(AuthContext authContext
            , SectionGroupConverter sectionGroupConverter
            , IPlacementTestResultRepository placementTestResultRepository
            , IUserService userService
            , IPlacementTestRepository placementTestRepository)
        {
            _authContext = authContext;
            _sectionGroupConverter = sectionGroupConverter;
            _placementTestResultRepository = placementTestResultRepository;
            _userService = userService;
            _placementTestRepository = placementTestRepository;
        }

        public async Task<MethodResult<PlacementTestBankModel>> Handle(GetPlacementTestQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<PlacementTestBankModel> methodResult = new MethodResult<PlacementTestBankModel>();

            var placementTests = await _placementTestRepository.Queryable.Include(x => x.PlacementTestSections)
                                                        .ThenInclude(x => x.SectionGroup)
                                                        .ThenInclude(x => x!.Sections)
                                                        .Where(x => x.Level == request.Level && x.IsActive)
                                                        .ToListAsync(cancellationToken);

            if (placementTests == null || placementTests.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(placementTests));
                return methodResult;
            }

            var studentResult = await _userService.GetStudentByUserIdWithCacheAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return methodResult;
            }
            var student = studentResult?.Content?.Result;
            var studentId = student?.Id;
            int age = DateTimeHelper.GetYearOld(student?.Human?.Birthday);
            var placementTestResultDone = await _placementTestResultRepository.Queryable.Where(x => x.Status == EnumResultStatus.Done && x.StudentId == studentId)
                                                                          .OrderByDescending(x => x.CreatedDate)
                                                                          .FirstOrDefaultAsync(cancellationToken);
            var placementTestResultInitial = await _placementTestResultRepository.Queryable.Where(x => x.StudentId == studentId)
                                                                        .OrderBy(x => x.CreatedDate)
                                                                        .FirstOrDefaultAsync(cancellationToken);

            if (placementTestResultDone != null)
            {
                var (levelNext, isLock) = placementTestResultDone.Level.GetLevelInScore(placementTestResultDone.Percent, IeltsScoreHelper.GetInitialAge(placementTestResultInitial?.Level, age));
                if (isLock)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumPlacementTestErrorCode.PlacementTestLock), nameof(levelNext));
                    return methodResult;
                }
            }
            var query = _placementTestRepository.Queryable.Where(x => x.Level == request.Level && x.IsActive);

            PlacementTestBankModel bankModel = new PlacementTestBankModel();
            bankModel.Level = request.Level;
            if (request.Level != EnumPlacementTestLevel.IELTS)
            {
                bankModel.SectionGroups = await RandomSectionGroup(query, cancellationToken);
            }
            else
            {
                bankModel.SectionGroups = await RandomSection(query, cancellationToken);
            }

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = bankModel;
            return methodResult;
        }

        public async Task<IList<SectionGroupModel>> RandomSectionGroup(IQueryable<PlacementTest> query, CancellationToken cancellationToken)
        {
            Random random = new Random();
            var skills = new List<EnumCourseSkill> { EnumCourseSkill.Reading, EnumCourseSkill.Listening, EnumCourseSkill.Vocabulary, EnumCourseSkill.Grammar };

            var results = new List<SectionGroupModel>();
            var placementTests = await query.Include(x => x.PlacementTestSections)
                                            .ThenInclude(x => x.SectionGroup)
                                            .ThenInclude(x => x!.Sections.OrderBy(x => x.DisplayOrder))
                                            .ThenInclude(x => x!.SectionQuestions)
                                            .ThenInclude(x => x.Question)
                                            .ToListAsync(cancellationToken);

            var sectionGroups = placementTests.SelectMany(x => x.PlacementTestSections)
                                                  .Where(x => x.SectionGroup != null)
                                                  .Select(x => x.SectionGroup!);

            foreach (var skill in skills)
            {
                var sectionGroup = sectionGroups.Where(x => x!.CourseSkill == skill).OrderBy(x => random.Next()).FirstOrDefault();
                var sectionGroupModel = _sectionGroupConverter.GetSectionGroupModel(sectionGroup, true);
                sectionGroupModel.Sections = sectionGroupModel.Sections?.OrderBy(x => x.DisplayOrder).ToList();
                if (sectionGroupModel != null)
                {
                    results.Add(sectionGroupModel);
                }
            }

            return results;
        }

        public async Task<IList<SectionGroupModel>> RandomSection(IQueryable<PlacementTest> query, CancellationToken cancellationToken)
        {
            Random random = new Random();
            var skills = new List<EnumCourseSkill> { EnumCourseSkill.Reading, EnumCourseSkill.Listening };

            var results = new List<SectionGroupModel>();
            var placementTests = await query.Include(x => x.PlacementTestSections)
                                    .ThenInclude(x => x.SectionGroup)
                                    .ThenInclude(x => x!.Sections.OrderBy(x => x.DisplayOrder))
                                    .ThenInclude(x => x.SectionParts)
                                    .ThenInclude(x => x!.SectionQuestions)
                                    .ThenInclude(x => x.Question)
                                    .ToListAsync(cancellationToken);

            var sectionGroups = placementTests.SelectMany(x => x.PlacementTestSections)
                                                  .Where(x => x.SectionGroup != null)
                                                  .Select(x => x.SectionGroup!);
            foreach (var skill in skills)
            {
                var sections = sectionGroups.Where(x => x!.CourseSkill == skill).SelectMany(x => x.Sections).OrderBy(x => random.Next());
                var sectionsResult = new List<Section>();
                for (int index = 1; index <= 4; index++)
                {
                    var section = sections.Where(x => x.DisplayOrder == index).FirstOrDefault();
                    if (section != null)
                    {
                        sectionsResult.Add(section);
                    }
                }

                var sectionGroupModel = _sectionGroupConverter.GetSectionGroupModel(new SectionGroup
                {
                    Sections = sectionsResult.OrderBy(x => x.DisplayOrder).ToList(),
                    CourseSkill = skill,
                    ExecutionTime = skill == EnumCourseSkill.Listening ? PlacementTestSettings.ListeningExecutionTime : PlacementTestSettings.ReadingExecutionTime,
                }, true);
                if (sectionGroupModel != null)
                {
                    results.Add(sectionGroupModel);
                }
            }

            return results;
        }
    }
}
