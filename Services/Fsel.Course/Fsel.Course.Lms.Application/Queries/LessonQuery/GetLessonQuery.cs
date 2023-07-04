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
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
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

            var studentsResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (studentsResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonErrorCode.UserNotExist));
                return methodResult;
            }
            var studentId = studentsResult.Content!.Result!.Id;

            var unit = await _unitRepository.Queryable.Include(x => x.UnitSkillMockTests)
                                                      .Include(x => x.UnitLessons)
                                                      .FirstOrDefaultAsync(x => x.Id == request.UnitId, cancellationToken);
            if (unit == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumUnitErrorCode.UnitNotExist), nameof(request.UnitId), request.UnitId);
                return methodResult;
            }

            var lessonResults = await _lessonResultRepository.Queryable.Where(x => x.UnitId == request.UnitId && x.CourseId == request.CourseId && x.StudentId == studentId).ToListAsync(cancellationToken);
            if (lessonResults == null || lessonResults.Count == 0)
            {
                lessonResults = unit.UnitLessons.OrderBy(x => x.DisplayOrder).Select(x => new LessonResult
                {
                    UnitId = x.UnitId,
                    LessonId = x.LessonId,
                    CourseId = request.CourseId,
                    Status = EnumResultStatus.Unfinished,
                    StudentId = studentId
                }).ToList();
                await _lessonResultRepository.AddList(lessonResults);
                await _lessonResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            var mockTestResults = await _mockTestResultRepository.Queryable.Where(x => x.UnitId == request.UnitId && x.CourseId == request.CourseId && x.StudentId == studentId).ToListAsync(cancellationToken);
            if (mockTestResults == null || mockTestResults.Count == 0)
            {
                mockTestResults = unit.UnitSkillMockTests.Select(x => new MockTestResult
                {
                    UnitId = x.UnitId,
                    MockTestId = x.MockTestId,
                    StudentId = studentId,
                    Status = EnumResultStatus.Unfinished,
                    CourseId = request.CourseId
                }).ToList();
                await _mockTestResultRepository.AddList(mockTestResults);
                await _mockTestResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            var lessons = await _lessonRepository.Queryable
                                .Include(x => x.LessonInstructions)
                                .Include(x => x.LessonResults)
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
                                    VideoId = x.LessonVideos.Where(x => x.Video != null).Select(x => x.Video).FirstOrDefault()!.Id,
                                    DisplayOrder = x.UnitLessons.Where(n => n.UnitId == request.UnitId && n.LessonId == x.Id).Select(x => x.DisplayOrder).FirstOrDefault(),
                                    LessonInstructions = _mapper.Map<IList<LessonInstructionModel>>(x.LessonInstructions.OrderBy(x => x.CreatedDate).ToList()),
                                    LessonResult = _mapper.Map<LessonResultModel>(x.LessonResults.Where(y => y.UnitId == request.UnitId && y.CourseId == request.CourseId && y.StudentId == studentId).FirstOrDefault(y => y.LessonId == x.Id)),
                                }).OrderBy(x => x.DisplayOrder).ToListAsync(cancellationToken: cancellationToken);

            if (lessons.Count == 0)
            {
                methodResult.AddErrorBadRequest(nameof(EnumLessonErrorCode.LessonNotExist), nameof(request.LessonId), request.LessonId);
                return methodResult;
            }

            var mocktest = await _unitRepository.Queryable
                                .Include(x => x.UnitSkillMockTests.Where(y => !y.IsDeleted))
                                .ThenInclude(x => x.MockTest)
                                .ThenInclude(x => x!.MockTestSections.Where(y => !y.IsDeleted))
                                .ThenInclude(x => x.SectionGroup)
                                .Include(x => x.MockTestResults.Where(y => y.UnitId == request.UnitId && y.CourseId == request.CourseId && y.StudentId == studentId))
                                .Where(x => x.Id == request.UnitId)
                                .AsNoTracking()
                                .SelectMany(x => x.UnitSkillMockTests)
                                .Select(x => x.MockTest)
                                .Select(x => new MockTestModel
                                {
                                    Id = x.Id,
                                    CourseType = x!.CourseType,
                                    Name = x.Name,
                                    TotalQuestion = x.MockTestSections.Select(x => x.SectionGroup).SelectMany(x => x!.Sections).SelectMany(x => x.SectionParts).SelectMany(x => x.SectionQuestions).Select(x => x.Question).Select(x => x!.CorrectTotal).Sum(),
                                    SectionGroups = x.MockTestSections.Select(x => x.SectionGroup).OrderBy(x => x.CreatedDate).Select(x => new SectionGroupModel
                                    {
                                        Id = x!.Id,
                                        CourseSkill = x.CourseSkill,
                                        ExecutionTime = x!.ExecutionTime,
                                    }).ToList(),
                                    MockTestResult = _mapper.Map<MockTestResultModel>(x.MockTestResults.FirstOrDefault(y => y.MockTestId == x.Id)),
                                }).FirstOrDefaultAsync(cancellationToken);

            methodResult.Result = new LessonsMockTestModel
            {
                Lessons = lessons,
                MockTest = mocktest
            };
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
