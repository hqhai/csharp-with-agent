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
    using Fsel.Course.Lms.Application.Services.UserServices;
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
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly AuthContext _authContext;

        public GetLessonQueryHandler(ILessonRepository lessonRepository,
            AuthContext authContext,
            IMapper mapper,
            IMockTestResultRepository mockTestResultRepository,
            ILessonResultRepository lessonResultRepository,
            IUserService userService,
            IUnitRepository unitRepository)
        {
            _lessonRepository = lessonRepository;
            _mapper = mapper;
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
            methodResult.Result = new LessonsMockTestModel();

            var studentsResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId).ConfigureAwait(false);
            if (studentsResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentsResult));
                return methodResult;
            }
            var studentId = studentsResult.Content?.Result?.Id;
            await UpdateLessonAndMockTest(request, studentId, cancellationToken).ConfigureAwait(false);
            methodResult.Result = new LessonsMockTestModel
            {
                Lessons = await GetLesson(request, studentId, cancellationToken).ConfigureAwait(false),
                MockTest = await GetMockTest(request, studentId, cancellationToken).ConfigureAwait(false)
            };

            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task UpdateLessonAndMockTest(GetLessonQuery request, Guid? studentId, CancellationToken cancellationToken)
        {
            var unit = await _unitRepository.Queryable.Include(x => x.UnitSkillMockTests)
                                                      .Include(x => x.UnitLessons)
                                                      .FirstOrDefaultAsync(x => x.Id == request.UnitId, cancellationToken);
            if (unit == null)
            {
                return;
            }
            var lessonResults = await _lessonResultRepository.Queryable.Where(x => x.UnitId == request.UnitId && x.CourseId == request.CourseId && x.StudentId == studentId).ToListAsync(cancellationToken);
            if (!lessonResults.Any())
            {
                lessonResults = unit.UnitLessons.OrderBy(x => x.DisplayOrder).Select((x, index) => new LessonResult
                {
                    UnitId = x.UnitId,
                    LessonId = x.LessonId,
                    CourseId = request.CourseId,
                    Status = index == 0 ? EnumResultStatus.New : EnumResultStatus.Unfinished,
                    StudentId = studentId ?? default
                }).ToList();
                await _lessonResultRepository.AddList(lessonResults);
                await _lessonResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
            }

            var mockTestResults = await _mockTestResultRepository.Queryable.Where(x => x.UnitId == request.UnitId && x.CourseId == request.CourseId && x.StudentId == studentId).ToListAsync(cancellationToken);
            if (!mockTestResults.Any())
            {
                mockTestResults = unit.UnitSkillMockTests.Select(x => new MockTestResult
                {
                    UnitId = x.UnitId,
                    MockTestId = x.MockTestId,
                    StudentId = studentId ?? default,
                    Status = EnumResultStatus.Unfinished,
                    CourseId = request.CourseId
                }).ToList();
                await _mockTestResultRepository.AddList(mockTestResults);
                await _mockTestResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
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
                            }).OrderBy(x => x.DisplayOrder).ToListAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var lessonIds = lessons.Select(x => x.Id).ToList();
            var lessonResults = await _lessonResultRepository.Queryable.Include(x => x.VideoResult)
                                                                     .Include(x => x.HomeWorkResults.Where(x => x.StudentId == studentId))
                                                                     .Include(x => x.ClassForumResults.Where(x => x.StudentId == studentId))
                                                                     .Where(x => x.StudentId == studentId && lessonIds.Contains(x.LessonId) && x.UnitId == request.UnitId && x.CourseId == request.CourseId)
                                                                     .AsNoTracking()
                                                                     .ToListAsync(cancellationToken).ConfigureAwait(false);

            foreach (var lesson in lessons)
            {
                var lessonResult = lessonResults.FirstOrDefault(x => x.LessonId == lesson.Id);
                if (lessonResult != null)
                {
                    lesson.LessonResult = _mapper.Map<LessonResultModel>(lessonResult);
                    var homeWorks = lessonResult.HomeWorkResults.Where(x => x.StudentId == studentId && x.LessonResultId == lessonResult.Id).ToList();
                    var classForumResult = lessonResult.ClassForumResults.FirstOrDefault(x => x.StudentId == studentId && x.LessonResultId == lessonResult.Id);
                    if (lessonResult.VideoResult?.Status == EnumResultStatus.Done)
                    {
                        lesson.IsClassForumLock = false;
                    }
                    if (classForumResult != null && (classForumResult.Status == EnumClassForumResultStatus.PendingForGrading || classForumResult.Status == EnumClassForumResultStatus.Graded))
                    {
                        lesson.IsHomeWorkLock = false;
                    }
                }
            }
            return lessons;
        }

        private async Task<MockTestModel?> GetMockTest(GetLessonQuery request, Guid? studentId, CancellationToken cancellationToken)
        {
            var unit = await _unitRepository.Queryable
                              .Include(x => x.UnitSkillMockTests.Where(y => !y.IsDeleted))
                              .ThenInclude(x => x.MockTest)
                              .ThenInclude(x => x!.MockTestSections.Where(y => !y.IsDeleted))
                              .ThenInclude(x => x.SectionGroup)
                               .Include(x => x.UnitSkillMockTests.Where(y => !y.IsDeleted))
                              .ThenInclude(x => x.MockTest)
                              .ThenInclude(x => x.MockTestResults.Where(y => y.UnitId == request.UnitId && y.CourseId == request.CourseId && y.StudentId == studentId))
                              .Where(x => x.Id == request.UnitId)
                              .AsNoTracking()
                              .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
            if (unit == null)
            {
                return null;
            }
            var mockTest = unit.UnitSkillMockTests.Select(x => x.MockTest)
                                    .Select(x =>
                                    {
                                        var sectionGroups = x.MockTestSections.Select(x => x.SectionGroup).ToList();
                                        var totalQuestion = sectionGroups.SelectMany(x => x!.Sections).SelectMany(x => x.SectionParts).SelectMany(x => x.SectionQuestions).Select(x => x.Question).Select(x => x!.CorrectTotal).Sum();
                                        var mockTestResult = x.MockTestResults.FirstOrDefault(y => y.MockTestId == x.Id && y.UnitId == request.UnitId && y.CourseId == request.CourseId && y.StudentId == studentId);
                                        var mockTest = new MockTestModel
                                        {
                                            Id = x!.Id,
                                            Name = x.Name,
                                            TotalQuestion = totalQuestion,
                                            SectionGroups = sectionGroups.OrderBy(x => x!.CreatedDate).Select(x => new SectionGroupModel
                                            {
                                                Id = x!.Id,
                                                CourseSkill = x.CourseSkill,
                                                ExecutionTime = x!.ExecutionTime,
                                            }).ToList(),
                                            MockTestResult = _mapper.Map<MockTestResultModel>(mockTestResult),
                                        };
                                        return mockTest;
                                    }).FirstOrDefault();
            return mockTest;
        }
    }
}
