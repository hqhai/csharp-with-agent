// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.LessonQuery
{
    using System;
    using System.Linq;
    using System.Linq.Dynamic.Core;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Infrastructure.Common;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetLessonQuery : IRequest<MethodResult<LessonsMockTestModel>>
    {
        public Guid CourseId { get; set; }
        public Guid UnitId { get; set; }
        public Guid? LessonId { get; set; }
    }

    public class GetLessonQueryHandler : IRequestHandler<GetLessonQuery, MethodResult<LessonsMockTestModel>>
    {
        private readonly ILessonRepository _lessonRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly IMockTestRepository _mockTestRepository;
        private readonly SectionGroupConverter _sectionGroupConverter;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly AuthContext _authContext;

        public GetLessonQueryHandler(ILessonRepository lessonRepository,
            AuthContext authContext,
            IMapper mapper,
            IMockTestRepository mockTestRepository,
            SectionGroupConverter sectionGroupConverter,
            IMockTestResultRepository mockTestResultRepository,
            ILessonResultRepository lessonResultRepository,
            IUserService userService,
            IUnitRepository unitRepository)
        {
            _lessonRepository = lessonRepository;
            _mapper = mapper;
            _mockTestRepository = mockTestRepository;
            _sectionGroupConverter = sectionGroupConverter;
            _mockTestResultRepository = mockTestResultRepository;
            _lessonResultRepository = lessonResultRepository;
            _authContext = authContext;
            _userService = userService;
            _unitRepository = unitRepository;
        }

        public async Task<MethodResult<LessonsMockTestModel>> Handle(GetLessonQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<LessonsMockTestModel> methodResult = new MethodResult<LessonsMockTestModel>();
            var studentsResult = await _userService.GetStudentByUserIdWithCacheAsync(_authContext.CurrentUserId).ConfigureAwait(false);
            if (studentsResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentsResult));
                return methodResult;
            }
            var studentId = studentsResult.Content?.Result?.Id;
            var unit = await _unitRepository.Queryable.Include(x => x.UnitResults.Where(x => x.StudentId == studentId && x.CourseId == request.CourseId))
                                 .Include(x => x.UnitSkillMockTests)
                                 .Include(x => x.UnitLessons)
                                 .FirstOrDefaultAsync(x => x.Id == request.UnitId, cancellationToken);
            if (unit == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(unit));
                return methodResult;
            }
            var unitResult = unit.UnitResults.FirstOrDefault();
            if (unitResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(unit));
                return methodResult;
            }
            else if (unitResult.Status == EnumResultStatus.Unfinished)
            {
                methodResult.AddErrorBadRequest(nameof(EnumResultErrorCode.ResultStatusUnfinished), nameof(unitResult.Status));
                return methodResult;
            }

            await UpdateLessonResults(request, studentId, unit, cancellationToken);
            await UpdateMockTestResults(request, studentId, unit, cancellationToken);
            methodResult.Result = new LessonsMockTestModel
            {
                Lessons = await GetLesson(request, studentId, cancellationToken),
                MockTest = unit.UnitSkillMockTests.Any() ? await GetMockTestAsync(request.CourseId, request.UnitId, studentId) : default
            };

            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<MockTestModel?> GetMockTestAsync(Guid courseId, Guid unitId, Guid? studentId)
        {
            var mockTest = await _mockTestRepository.Queryable.Include(x => x.MockTestSections)
                                       .ThenInclude(x => x.SectionGroup)
                                       .ThenInclude(x => x!.Sections)
                                       .ThenInclude(x => x.SectionParts)
                                       .ThenInclude(x => x.SectionQuestions)
                                       .Include(x => x.UnitSkillMockTests.Where(y => !y.IsDeleted))
                                       .Include(x => x.MockTestResults.Where(y => y.UnitId == unitId && y.CourseId == courseId && y.StudentId == studentId))
                                       .Where(x => x.UnitSkillMockTests.Any(x => x.UnitId == unitId))
                                       .AsNoTracking()
                                       .FirstOrDefaultAsync();
            if (mockTest == null)
            {
                return default;
            }
            var sectionGroup = mockTest.MockTestSections.Select(x => x.SectionGroup).FirstOrDefault();
            var mockTestModel = _mapper.Map<MockTestModel>(mockTest);
            var sectionGroupModel = _mapper.Map<SectionGroupModel>(sectionGroup);
            sectionGroupModel.TotalQuestion = _sectionGroupConverter.GetTotalQuestion(sectionGroup!.Sections.ToList(), sectionGroup!.CourseSkill);
            mockTestModel.TotalQuestion = _sectionGroupConverter.GetTotalQuestion(mockTest.MockTestSections.Select(x => x.SectionGroup!).ToList());
            mockTestModel.MockTestResult = _mapper.Map<MockTestResultModel>(mockTest.MockTestResults.FirstOrDefault());
            mockTestModel.SectionGroups = new List<SectionGroupModel> { sectionGroupModel };
            return mockTestModel;
        }

        private async Task UpdateLessonResults(GetLessonQuery request, Guid? studentId, Domain.Entities.Unit unit, CancellationToken cancellationToken)
        {
            var isUsedLessonResult = await _lessonResultRepository.Queryable.AnyAsync(x => x.UnitId == request.UnitId && x.CourseId == request.CourseId && x.StudentId == studentId, cancellationToken);
            if (!isUsedLessonResult)
            {
                var lessonResults = unit.UnitLessons.OrderBy(x => x.DisplayOrder).Select((x, index) => new LessonResult
                {
                    UnitId = x.UnitId,
                    LessonId = x.LessonId,
                    CourseId = request.CourseId,
                    Status = index == 0 ? EnumResultStatus.New : EnumResultStatus.Unfinished,
                    StudentId = studentId ?? default
                }).ToList();
                await _lessonResultRepository.BulkMergeAsync(lessonResults, bulk =>
                {
                    bulk.ColumnPrimaryKeyExpression = c => new { c.CourseId, c.StudentId, c.UnitId, c.LessonId, c.IsDeleted };
                });
            }
        }

        private async Task UpdateMockTestResults(GetLessonQuery request, Guid? studentId, Domain.Entities.Unit unit, CancellationToken cancellationToken)
        {
            var isMockTestResult = await _mockTestResultRepository.Queryable.AnyAsync(x => x.UnitId == request.UnitId && x.CourseId == request.CourseId && x.StudentId == studentId, cancellationToken);
            if (!isMockTestResult)
            {
                var mockTestResults = unit.UnitSkillMockTests.Select(x => new MockTestResult
                {
                    UnitId = x.UnitId,
                    MockTestId = x.MockTestId,
                    StudentId = studentId ?? default,
                    Status = EnumResultStatus.Unfinished,
                    CourseId = request.CourseId
                }).ToList();

                await _mockTestResultRepository.BulkMergeAsync(mockTestResults, bulk =>
                {
                    bulk.ColumnPrimaryKeyExpression = c => new { c.CourseId, c.StudentId, c.UnitId, c.MockTestId, c.IsDeleted };
                });
            }
        }

        private async Task<IList<LessonModel>> GetLesson(GetLessonQuery request, Guid? studentId, CancellationToken cancellationToken)
        {
            var lessons = await _lessonRepository.Queryable
                            .Include(x => x.LessonInstructions)
                            .Include(x => x.UnitLessons)
                            .Include(x => x.LessonVideos)
                            .ThenInclude(x => x.Video)
                            .Where(x => x.UnitLessons.Any(x => x.UnitId == request.UnitId))
                            .Where(x => !request.LessonId.HasValue || x.Id == request.LessonId)
                            .AsNoTracking()
                            .Select(x => new LessonModel
                            {
                                Id = x.Id,
                                Name = x.Name,
                                CourseLevel = x.CourseLevel,
                                InstructionContent = x.InstructionContent,
                                IsActive = x.UnitLessons.Any(),
                                VideoId = x.LessonVideos.FirstOrDefault()!.VideoId,
                                DisplayOrder = x.UnitLessons.Where(n => n.UnitId == request.UnitId && n.LessonId == x.Id).Select(x => x.DisplayOrder).FirstOrDefault(),
                                LessonInstructions = _mapper.Map<IList<LessonInstructionModel>>(x.LessonInstructions.OrderBy(x => x.CreatedDate).ToList()),
                            }).OrderBy(x => x.DisplayOrder).ToListAsync(cancellationToken: cancellationToken);
            var lessonIds = lessons.Select(x => x.Id).ToList();
            var lessonResults = await _lessonResultRepository.Queryable.Include(x => x.VideoResult)
                                                        .Include(x => x.ClassForumResults.Where(x => x.StudentId == studentId))
                                                        .WhereBulkContains(lessonIds, x => x.LessonId)
                                                        .Where(x => x.StudentId == studentId && x.UnitId == request.UnitId && x.CourseId == request.CourseId)
                                                        .AsNoTracking()
                                                        .ToListAsync(cancellationToken);
            return lessons.Select(x =>
            {
                var lessonResult = lessonResults.FirstOrDefault(y => y.LessonId == x.Id);
                if (lessonResult != null)
                {
                    x.LessonResult = _mapper.Map<LessonResultModel>(lessonResult);
                    var classForumResult = lessonResult.ClassForumResults.FirstOrDefault();
                    if (lessonResult.VideoResult?.Status == EnumResultStatus.Done)
                    {
                        x.IsClassForumLock = false;
                    }
                    if (classForumResult != null && (classForumResult.Status != EnumClassForumResultStatus.Draft))
                    {
                        x.IsHomeWorkLock = false;
                    }
                }
                return x;
            }).ToList();
        }
    }
}
