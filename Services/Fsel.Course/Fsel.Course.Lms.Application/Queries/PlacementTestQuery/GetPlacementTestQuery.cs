// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.PlacementTestQuery
{
    using System;
    using System.Collections.Generic;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetPlacementTestQuery : IRequest<MethodResult<PlacementTestBankModel>>
    {
        public EnumPlacementTestLevel Level { get; set; }
    }

    public class StartPlacementTestCommandHandler : IRequestHandler<GetPlacementTestQuery, MethodResult<PlacementTestBankModel>>
    {
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly IPlacementTestRepository _placementTestRepository;

        public StartPlacementTestCommandHandler(AuthContext authContext
            , IUserService userService
            , IMapper mapper
            , IPlacementTestRepository placementTestRepository)
        {
            _authContext = authContext;
            _userService = userService;
            _mapper = mapper;
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
                methodResult.AddErrorBadRequest(nameof(EnumPlacementTestErrorCode.PlacementTestNotExist));
                return methodResult;
            }

            var student = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!student.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonErrorCode.UserNotExist), nameof(student), _authContext.CurrentUserId.ToString());
                return methodResult;
            }
            var studentId = student?.Content?.Result?.Id;

            PlacementTestBankModel bankModel = new PlacementTestBankModel();
            bankModel.Level = request.Level;
            if (request.Level != EnumPlacementTestLevel.IELTS)
            {
                placementTests = await _placementTestRepository.Queryable.Include(x => x.PlacementTestSections)
                                                        .ThenInclude(x => x.SectionGroup)
                                                        .ThenInclude(x => x!.Sections)
                                                        .ThenInclude(x => x!.SectionQuestions)
                                                        .ThenInclude(x => x.Question)
                                                        .Where(x => x.Level == request.Level && x.IsActive)
                                                        .ToListAsync(cancellationToken);
                var sectionGroupReading = RamdomSkillSectionGroup(placementTests, EnumCourseSkill.Reading);
                var sectionGroupListening = RamdomSkillSectionGroup(placementTests, EnumCourseSkill.Listening);
                var sectionGroupVocabulary = RamdomSkillSectionGroup(placementTests, EnumCourseSkill.Vocabulary);
                var sectionGroupGrammar = RamdomSkillSectionGroup(placementTests, EnumCourseSkill.Grammar);

                if (sectionGroupReading == null || sectionGroupGrammar == null || sectionGroupListening == null || sectionGroupVocabulary == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumPlacementTestErrorCode.PlacementTestNull));
                    return methodResult;
                }

                List<SectionGroupModel> sectionGroups = new List<SectionGroupModel>();

                sectionGroups.Add(sectionGroupReading);
                sectionGroups.Add(sectionGroupListening);
                sectionGroups.Add(sectionGroupVocabulary);
                sectionGroups.Add(sectionGroupGrammar);
                bankModel.SectionGroups = sectionGroups;
            }
            else
            {
                placementTests = await _placementTestRepository.Queryable.Include(x => x.PlacementTestSections)
                                                        .ThenInclude(x => x.SectionGroup)
                                                        .ThenInclude(x => x!.Sections)
                                                        .ThenInclude(x => x.SectionParts)
                                                        .ThenInclude(x => x!.SectionQuestions)
                                                        .ThenInclude(x => x.Question)
                                                        .Where(x => x.Level == request.Level && x.IsActive)
                                                        .ToListAsync(cancellationToken);
                var sectionReading = RamdomSkillSection(placementTests, EnumCourseSkill.Reading);
                var sectionListening = RamdomSkillSection(placementTests, EnumCourseSkill.Listening);

                if (sectionReading == null || sectionListening == null)
                {
                    methodResult.AddErrorBadRequest(nameof(EnumPlacementTestErrorCode.PlacementTestNull));
                    return methodResult;
                }
                List<PlacementTestBankSkillModel> placementTestBankSkills = new List<PlacementTestBankSkillModel>();
                placementTestBankSkills.Add(sectionReading);
                placementTestBankSkills.Add(sectionListening);
                bankModel.PlacementSkills = placementTestBankSkills;
            }

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = bankModel;
            return methodResult;
        }

        public SectionGroupModel? RamdomSkillSectionGroup(IList<PlacementTest>? listPlacementTests, EnumCourseSkill courseSkill)
        {
            Random random = new Random();
            var sectionGroup = listPlacementTests?.SelectMany(x => x.PlacementTestSections)
                                                  .Select(x => x.SectionGroup)
                                                  .Where(x => x!.CourseSkill == courseSkill)
                                                  .OrderBy(x => random.Next()).FirstOrDefault();
            if (sectionGroup == null)
            {
                return null;
            }
            SectionGroupModel sectionGroupModel = _mapper.Map<SectionGroupModel>(sectionGroup);
            sectionGroupModel.Sections = sectionGroup.Sections.Select(x => new SectionModel
            {
                Id = x.Id,
                Name = x.Name,
                MediaPost = x.MediaPost,
                TargetWord = x.TargetWord,
                Questions = x.SectionQuestions.Select(x => x.Question).Select(x => new QuestionModel
                {
                    Id = x!.Id,
                    QuestionType = x.QuestionType,
                    Explanation = x.Explanation,
                    Ungraded = x.Ungraded,
                    CorrectTotal = x.CorrectTotal,
                    Config = x.Config
                }).ToList(),
            }).ToList();

            return sectionGroupModel;
        }

        public PlacementTestBankSkillModel? RamdomSkillSection(IList<PlacementTest>? listPlacementTests, EnumCourseSkill courseSkill)
        {
            Random random = new Random();
            var sectionGroups = listPlacementTests?.SelectMany(x => x.PlacementTestSections)
                                                  .Select(x => x.SectionGroup)
                                                  .Where(x => x!.CourseSkill == courseSkill)
                                                  .ToList();
            if (sectionGroups == null || sectionGroups.Count == 0)
            {
                return null;
            }
            List<SectionModel> sectionModels = new List<SectionModel>();
            var sections = sectionGroups.SelectMany(x => x!.Sections).ToList();
            var section1 = sections.Where(x => x.DisplayOrder == 1).OrderBy(x => random.Next()).FirstOrDefault();
            var section2 = sections.Where(x => x.DisplayOrder == 2).OrderBy(x => random.Next()).FirstOrDefault();
            var section3 = sections.Where(x => x.DisplayOrder == 3).OrderBy(x => random.Next()).FirstOrDefault();
            var sectionModel1 = GetSectionModel(section1, sections);
            var sectionModel2 = GetSectionModel(section2, sections);
            var sectionModel3 = GetSectionModel(section3, sections);
            sectionModels.Add(sectionModel1);
            sectionModels.Add(sectionModel2);
            sectionModels.Add(sectionModel3);
            if (sections.Count % 4 == 0)
            {
                var section4 = sections.Where(x => x.DisplayOrder == 4).OrderBy(x => random.Next()).FirstOrDefault();
                var sectionModel4 = _mapper.Map<SectionModel>(section4);
                sectionModels.Add(sectionModel4);
            }
            PlacementTestBankSkillModel placementTestBankSkill = new PlacementTestBankSkillModel { CourseSkill = courseSkill, Sections = sectionModels };
            return placementTestBankSkill;
        }

        public SectionModel GetSectionModel(Section? section, IList<Section> sections)
        {
            var sectionModel = _mapper.Map<SectionModel>(section);
            sectionModel.SectionParts = sections.SelectMany(x => x.SectionParts).Select(x => new SectionPartModel
            {
                Id = x.Id,
                PartName = x.PartName,
                SectionId = x.SectionId,
                Question = x.SectionQuestions.Select(x => x.Question).Select(x => new QuestionModel
                {
                    Id = x!.Id,
                    QuestionType = x.QuestionType,
                    Explanation = x.Explanation,
                    Ungraded = x.Ungraded,
                    CorrectTotal = x.CorrectTotal,
                    Config = x.Config
                }).ToList()
            }).ToList();
            return sectionModel;
        }
    }
}
