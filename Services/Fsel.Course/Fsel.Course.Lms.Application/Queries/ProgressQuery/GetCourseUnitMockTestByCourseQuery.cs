// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ProgressQuery
{
    using System.Threading;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Helpers;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetCourseUnitMockTestByCourseQuery : IRequest<MethodResult<IList<CourseUnitMockTestResultModel>>>
    {
        public Guid CourseId { get; set; }
        public EnumLearnProcessType Type { get; set; }
    }

    public class GetCourseUnitMockTestByCourseQueryHandler : IRequestHandler<GetCourseUnitMockTestByCourseQuery, MethodResult<IList<CourseUnitMockTestResultModel>>>
    {
        private readonly AuthContext _authContext;
        private readonly IMapper _mapper;
        private readonly ILessonRepository _lessonRepository;
        private readonly IVideoRepository _videoRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly IFinalTestRepository _finalTestRepository;
        private readonly IMockTestRepository _mockTestRepository;
        private readonly IUserService _userService;

        public GetCourseUnitMockTestByCourseQueryHandler(AuthContext authContext
            , IMapper mapper
            , ILessonRepository lessonRepository
            , IVideoRepository videoRepository
            , ICourseRepository courseRepository
            , IUnitRepository unitRepository
            , IFinalTestRepository finalTestRepository
            , IMockTestRepository mockTestRepository
            , IUserService userService)
        {
            _authContext = authContext;
            _mapper = mapper;
            _lessonRepository = lessonRepository;
            _videoRepository = videoRepository;
            _courseRepository = courseRepository;
            _unitRepository = unitRepository;
            _finalTestRepository = finalTestRepository;
            _mockTestRepository = mockTestRepository;
            _userService = userService;
        }

        public async Task<MethodResult<IList<CourseUnitMockTestResultModel>>> Handle(GetCourseUnitMockTestByCourseQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<CourseUnitMockTestResultModel>> methodResult = new MethodResult<IList<CourseUnitMockTestResultModel>>();
            var studentResult = await _userService.GetStudentByUserIdWithCacheAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentResult));
                return methodResult;
            }
            var student = studentResult?.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }
            var course = await _courseRepository.ReadQueryable.Include(x => x.CourseUnitMockTests).FirstOrDefaultAsync(x => x.Id == request.CourseId, cancellationToken);
            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(course));
                return methodResult;
            }
            else if (course.CourseType == EnumCourseType.Ielts && request.Type == EnumLearnProcessType.UnitTest)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.CourseNotTypeAcademic), nameof(course));
                return methodResult;
            }

            List<CourseUnitMockTestResultModel>? courseUnitMockTestResults = default;
            if (course.CourseType == EnumCourseType.Academic)
            {
                courseUnitMockTestResults = await GetListToAcademicAsync(course, request.Type, student.Id);
            }
            else
            {
                courseUnitMockTestResults = await GetListToIELTSAsync(course, request.Type, student.Id);
            }

            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = courseUnitMockTestResults;
            return methodResult;
        }

        private async Task<IList<Domain.Entities.Unit>> GetUnitsAsync(Guid courseId, Guid studentId)
        {
            return await _unitRepository.Queryable.Include(x => x.UnitResults.Where(x => x.StudentId == studentId && x.CourseId == courseId))
                                                  .Include(x => x.UnitSkillMockTests)
                                                  .Where(x => x.CourseUnitMockTests.Any(x => x.CourseId == courseId))
                                                  .ToListAsync();
        }

        private async Task<List<CourseUnitMockTestResultModel>> GetListToAcademicAsync(Course course, EnumLearnProcessType type, Guid studentId)
        {
            var units = await GetUnitsAsync(course.Id, studentId);
            var finalTest = await _finalTestRepository.ReadQueryable
                                        .Include(x => x.FinalTestResults.Where(x => x.StudentId == studentId && x.CourseId == course.Id))
                                        .ThenInclude(x => x.SectionGroupResults.Where(x => x.StudentId == studentId))
                                        .Where(x => x.CourseUnitMockTests.Any(x => x.CourseId == course.Id))
                                        .FirstOrDefaultAsync();
            var listUnit = new List<CourseUnitMockTestResultModel>();
            foreach (var courseUnitMockTest in course.CourseUnitMockTests.OrderBy(x => x.DisplayOrder))
            {
                double percent = 0;
                CourseUnitMockTestResultModel? courseUnitMockTestResult = default;
                if (courseUnitMockTest.UnitId.HasValue)
                {
                    var unit = units.FirstOrDefault(x => x.Id == courseUnitMockTest.UnitId.Value);
                    if (unit != null)
                    {
                        switch (type)
                        {
                            case EnumLearnProcessType.LessonVideo:
                                percent = await _lessonRepository.GetPercentLesson(course.Id, unit.Id, studentId);
                                break;

                            case EnumLearnProcessType.HomeWork:
                                percent = await _lessonRepository.GetPercentHomeWork(course.Id, unit.Id, studentId);
                                break;

                            case EnumLearnProcessType.ClassForum:
                                percent = await _lessonRepository.GetPercentClassForum(course.Id, unit.Id, studentId);
                                break;

                            case EnumLearnProcessType.UnitTest:
                                percent = await _videoRepository.GetPercent(course.Id, unit.Id, studentId);
                                break;

                            default:
                                throw new NotImplementedException();
                        }
                        courseUnitMockTestResult = GetCourseUnitMockTestResultModel(unit, percent, courseUnitMockTest.DisplayOrder);
                    }
                }
                else if (courseUnitMockTest.FinalTestId.HasValue && finalTest != null)
                {
                    courseUnitMockTestResult = GetCourseUnitMockTestResultModel(finalTest, courseUnitMockTest.DisplayOrder);
                }
                if (courseUnitMockTestResult != null)
                {
                    listUnit.Add(courseUnitMockTestResult);
                }
            }
            return listUnit.OrderBy(x => x.DisplayOrder).ToList();
        }

        private async Task<List<CourseUnitMockTestResultModel>> GetListToIELTSAsync(Course course, EnumLearnProcessType type, Guid studentId)
        {
            var units = await GetUnitsAsync(course.Id, studentId);
            var mockTests = await _mockTestRepository.ReadQueryable.Include(x => x.MockTestSections)
                                                     .ThenInclude(x => x.SectionGroup)
                                                     .Include(x => x.MockTestResults.Where(x => x.StudentId == studentId && x.CourseId == course.Id))
                                                     .ThenInclude(x => x.SectionGroupResults.Where(x => x.StudentId == studentId))
                                                     .Where(x => x.CourseUnitMockTests.Any(x => x.CourseId == course.Id))
                                                     .ToListAsync();
            var listUnit = new List<CourseUnitMockTestResultModel>();
            foreach (var courseUnitMockTest in course.CourseUnitMockTests.OrderBy(x => x.DisplayOrder))
            {
                double percent = 0;
                CourseUnitMockTestResultModel? courseUnitMockTestResult = default;
                if (courseUnitMockTest.UnitId.HasValue)
                {
                    var unit = units.FirstOrDefault(x => x.Id == courseUnitMockTest.UnitId.Value);
                    if (unit != null)
                    {
                        switch (type)
                        {
                            case EnumLearnProcessType.LessonVideo:
                                percent = await _lessonRepository.GetPercentLesson(course.Id, unit.Id, studentId);
                                break;

                            case EnumLearnProcessType.HomeWork:
                                percent = await _lessonRepository.GetPercentHomeWork(course.Id, unit.Id, studentId);
                                break;

                            case EnumLearnProcessType.ClassForum:
                                percent = await _lessonRepository.GetPercentClassForum(course.Id, unit.Id, studentId);
                                break;

                            case EnumLearnProcessType.UnitTest:
                                percent = await _videoRepository.GetPercent(course.Id, unit.Id, studentId);
                                break;

                            default:
                                throw new NotImplementedException();
                        }
                        courseUnitMockTestResult = GetCourseUnitMockTestResultModel(unit, percent, courseUnitMockTest.DisplayOrder);
                    }
                }
                else if (courseUnitMockTest.MockTestId.HasValue)
                {
                    var mockTest = mockTests.FirstOrDefault(x => x.Id == courseUnitMockTest.MockTestId.Value);
                    if (mockTest != null)
                    {
                        courseUnitMockTestResult = GetCourseUnitMockTestResultModel(mockTest, courseUnitMockTest.DisplayOrder);
                    }
                }
                if (courseUnitMockTestResult != null)
                {
                    listUnit.Add(courseUnitMockTestResult);
                }
            }
            return listUnit.OrderBy(x => x.DisplayOrder).ToList();
        }

        private CourseUnitMockTestResultModel GetCourseUnitMockTestResultModel(Domain.Entities.Unit unit, double percent, int displayOrder)
        {
            var courseUnitMockTestResult = _mapper.Map<CourseUnitMockTestResultModel>(unit.UnitResults.FirstOrDefault());
            courseUnitMockTestResult.ProgressPercent = percent;
            courseUnitMockTestResult.Code = unit.Code;
            courseUnitMockTestResult.Name = unit.Name;
            courseUnitMockTestResult.DisplayOrder = displayOrder;
            courseUnitMockTestResult.Type = nameof(Domain.Entities.Unit);
            courseUnitMockTestResult.MockTestId = unit.UnitSkillMockTests.FirstOrDefault()?.MockTestId;
            return courseUnitMockTestResult;
        }

        private CourseUnitMockTestResultModel GetCourseUnitMockTestResultModel(FinalTest finalTest, int displayOrder)
        {
            int maxSkillFinalTest = 3;
            var finalTestResult = finalTest.FinalTestResults.FirstOrDefault();
            var courseUnitMockTestResult = _mapper.Map<CourseUnitMockTestResultModel>(finalTestResult);
            if (finalTestResult != null)
            {
                var countSkillDone = finalTestResult.SectionGroupResults.Where(x => x.Status == EnumResultStatus.Done).Count();
                courseUnitMockTestResult.ProgressPercent = NumberHelper.GetPercent(countSkillDone, maxSkillFinalTest);
            }
            courseUnitMockTestResult.Name = finalTest.Name;
            courseUnitMockTestResult.DisplayOrder = displayOrder;
            courseUnitMockTestResult.Type = nameof(FinalTest);
            return courseUnitMockTestResult;
        }

        private CourseUnitMockTestResultModel GetCourseUnitMockTestResultModel(MockTest mockTest, int displayOrder)
        {
            int maxSkillMockTest = 4;
            var mockTestResult = mockTest.MockTestResults.FirstOrDefault();
            var courseUnitMockTestResult = _mapper.Map<CourseUnitMockTestResultModel>(mockTestResult);
            if (mockTestResult != null)
            {
                var countSkillDone = mockTestResult.SectionGroupResults.Where(x => x.Status == EnumResultStatus.Done).Count();
                courseUnitMockTestResult.ProgressPercent = NumberHelper.GetPercent(countSkillDone, maxSkillMockTest);
            }
            courseUnitMockTestResult.Name = mockTest.Name;
            courseUnitMockTestResult.DisplayOrder = displayOrder;
            courseUnitMockTestResult.Type = nameof(MockTest);
            return courseUnitMockTestResult;
        }
    }
}
