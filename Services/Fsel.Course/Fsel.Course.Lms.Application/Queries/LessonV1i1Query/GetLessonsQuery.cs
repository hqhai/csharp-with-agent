// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.LessonV1i1Query
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Entities;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels.V1i1;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetLessonsQuery : IRequest<MethodResult<IList<LessonMockTestResultModel>>>
    {
        public Guid CourseId { get; set; }
        public Guid UnitId { get; set; }
    }

    public class GetLessonsQueryHandler : IRequestHandler<GetLessonsQuery, MethodResult<IList<LessonMockTestResultModel>>>
    {
        private readonly IUnitRepository _unitRepository;
        private readonly IUserService _userService;
        private readonly IMockTestResultRepository _mockTestResultRepository;
        private readonly IMapper _mapper;
        private readonly ILessonResultRepository _lessonResultRepository;
        private readonly AuthContext _authContext;

        public GetLessonsQueryHandler(AuthContext authContext,
            IMapper mapper,
            ILessonResultRepository lessonResultRepository,
            IUserService userService,
            IMockTestResultRepository mockTestResultRepository,
            IUnitRepository unitRepository)
        {
            _mapper = mapper;
            _lessonResultRepository = lessonResultRepository;
            _authContext = authContext;
            _userService = userService;
            _mockTestResultRepository = mockTestResultRepository;
            _unitRepository = unitRepository;
        }

        public async Task<MethodResult<IList<LessonMockTestResultModel>>> Handle(GetLessonsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<LessonMockTestResultModel>>();
            var studentsResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (studentsResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentsResult));
                return methodResult;
            }
            var studentId = studentsResult.Content?.Result?.Id;
            var unit = await _unitRepository.Queryable.Include(x => x.UnitResults.Where(x => x.StudentId == studentId && x.CourseId == request.CourseId))
                                 .Include(x => x.UnitLessons)
                                 .Include(x => x.UnitSkillMockTests)
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
            var data = new List<LessonMockTestResultModel>();
            data.AddRange(await UpdateLessonResults(request, studentId, unit, cancellationToken));
            if (unit.UnitSkillMockTests.Any())
            {
                data.Add(await UpdateMockTestResults(request, studentId, unit, cancellationToken));
            }
            methodResult.Result = data;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }

        private async Task<IList<LessonMockTestResultModel>> UpdateLessonResults(GetLessonsQuery request, Guid? studentId, Domain.Entities.Unit unit, CancellationToken cancellationToken)
        {
            var lessonResults = await _lessonResultRepository.Queryable.Where(x => x.UnitId == request.UnitId && x.CourseId == request.CourseId && x.StudentId == studentId).OrderBy(x => x.CreatedDate).ToListAsync(cancellationToken);
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
                await _lessonResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            return _mapper.Map<IList<LessonMockTestResultModel>>(lessonResults.OrderBy(x => x.CreatedDate).ToList());
        }

        private async Task<LessonMockTestResultModel> UpdateMockTestResults(GetLessonsQuery request, Guid? studentId, Domain.Entities.Unit unit, CancellationToken cancellationToken)
        {
            var mockTestResult = await _mockTestResultRepository.Queryable.FirstOrDefaultAsync(x => x.UnitId == request.UnitId && x.CourseId == request.CourseId && x.StudentId == studentId, cancellationToken);
            if (mockTestResult == null)
            {
                mockTestResult = unit.UnitSkillMockTests.Select(x => new MockTestResult
                {
                    UnitId = x.UnitId,
                    MockTestId = x.MockTestId,
                    StudentId = studentId ?? default,
                    Status = EnumResultStatus.Unfinished,
                    CourseId = request.CourseId
                }).FirstOrDefault();
                _mockTestResultRepository.Add(mockTestResult);
                await _mockTestResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
            }
            return _mapper.Map<LessonMockTestResultModel>(mockTestResult);
        }
    }
}
