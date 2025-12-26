// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ProgressQuery
{
    using System.Threading;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetUnitByUnitQuery : IRequest<MethodResult<IList<UnitModel>>>
    {
        public Guid CourseId { get; set; }
        public EnumLearnProcessType Type { get; set; }
    }

    public class GetUnitByUnitVideoQueryHandler : IRequestHandler<GetUnitByUnitQuery, MethodResult<IList<UnitModel>>>
    {
        private readonly AuthContext _authContext;
        private readonly IMapper _mapper;
        private readonly ILessonRepository _lessonRepository;
        private readonly IVideoRepository _videoRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly IUserService _userService;

        public GetUnitByUnitVideoQueryHandler(AuthContext authContext
            , IMapper mapper
            , ILessonRepository lessonRepository
            , IVideoRepository videoRepository
            , ICourseRepository courseRepository
            , IUnitRepository unitRepository
            , IUserService userService)
        {
            _authContext = authContext;
            _mapper = mapper;
            _lessonRepository = lessonRepository;
            _videoRepository = videoRepository;
            _courseRepository = courseRepository;
            _unitRepository = unitRepository;
            _userService = userService;
        }

        public async Task<MethodResult<IList<UnitModel>>> Handle(GetUnitByUnitQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<UnitModel>> methodResult = new MethodResult<IList<UnitModel>>();
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
            var studentId = student.Id;
            var course = await _courseRepository.GetByIdAsync(request.CourseId);
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
            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = await GetListAsync(request, studentId);
            return methodResult;
        }

        private async Task<IList<UnitModel>?> GetListAsync(GetUnitByUnitQuery request, Guid? studentId)
        {
            var units = await _unitRepository.ReadQueryable.Include(x => x.UnitResults.Where(x => x.StudentId == studentId && x.CourseId == request.CourseId))
                                                       .Include(x => x.CourseUnitMockTests.Where(x => !x.IsDeleted && x.CourseId == request.CourseId))
                                                       .Where(x => x.CourseUnitMockTests.Any(x => x.CourseId == request.CourseId))
                                                       .ToListAsync();

            if (units.Any())
            {
                var listUnit = new List<UnitModel>();
                foreach (var unit in units)
                {
                    double percent = 0;
                    switch (request.Type)
                    {
                        case EnumLearnProcessType.LessonVideo:
                            percent = await _lessonRepository.GetPercentLesson(request.CourseId, unit.Id, studentId);
                            break;

                        case EnumLearnProcessType.HomeWork:
                            percent = await _lessonRepository.GetPercentHomeWork(request.CourseId, unit.Id, studentId);
                            break;

                        case EnumLearnProcessType.ClassForum:
                            percent = await _lessonRepository.GetPercentClassForum(request.CourseId, unit.Id, studentId);
                            break;

                        case EnumLearnProcessType.UnitTest:
                            percent = await _videoRepository.GetPercent(request.CourseId, unit.Id, studentId);
                            break;

                        default:
                            throw new NotImplementedException();
                    }
                    listUnit.Add(GetUnitModel(unit, request.CourseId, percent));
                }
                return listUnit.OrderBy(x => x.DisplayOrder).ToList();
            }
            return default;
        }

        private UnitModel GetUnitModel(Domain.Entities.Unit x, Guid courseId, double percent)
        {
            var displayOrder = x.CourseUnitMockTests.FirstOrDefault(y => y.CourseId == courseId && y.UnitId == x.Id)?.DisplayOrder ?? default;
            var unitModel = _mapper.Map<UnitModel>(x);
            unitModel.DisplayOrder = displayOrder;
            unitModel.IsActive = x.CourseUnitMockTests.Any();
            unitModel.Percent = percent;
            unitModel.UnitResult = _mapper.Map<UnitResultModel>(x.UnitResults.FirstOrDefault());
            return unitModel;
        }
    }
}
