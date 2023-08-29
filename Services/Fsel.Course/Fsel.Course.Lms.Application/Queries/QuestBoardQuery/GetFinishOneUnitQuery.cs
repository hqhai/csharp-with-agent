// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.QuestBoardQuery
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Lms.Application.Services.OrderServices;
    using Fsel.Course.Lms.Application.Services.OrderServices.Model;
    using Fsel.Course.Lms.Application.Services.TrainingServices;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetFinishOneUnitQuery : IRequest<MethodResult<(double, Guid?)>>
    {
        public Guid StudentId { get; set; }
        public Guid CurrentUserId { get; set; }
    }

    public class GetFinishOneUnitQueryHandler : IRequestHandler<GetFinishOneUnitQuery, MethodResult<(double, Guid?)>>
    {
        private readonly ITrainingService _trainingService;
        private readonly IOrderService _orderService;
        private readonly IUnitRepository _unitRepository;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly ICourseRepository _courseRepository;

        public GetFinishOneUnitQueryHandler(ITrainingService trainingService
            , IOrderService orderService
            , IUnitRepository unitRepository
            , IUnitResultRepository unitResultRepository
            , ICourseRepository courseRepository)
        {
            _trainingService = trainingService;
            _orderService = orderService;
            _unitRepository = unitRepository;
            _unitResultRepository = unitResultRepository;
            _courseRepository = courseRepository;
        }

        public async Task<MethodResult<(double, Guid?)>> Handle(GetFinishOneUnitQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<(double, Guid?)> methodResult = new MethodResult<(double, Guid?)>();
            var classResult = await _trainingService.GetClassByStudentId(request.StudentId);
            if (!classResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallTrainingServiceError));
                return methodResult;
            }
            var @class = classResult?.Content?.Result;
            if (@class == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(@class));
                return methodResult;
            }

            var orderResult = await _orderService.IsCheckStatusUser(new IsCheckPaymentStatusByUserModel { ClassId = @class.Id, CourseId = @class.CourseId, PackageId = @class.PackageId, UserId = request.CurrentUserId });
            if (!orderResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallOrderServiceError));
                return methodResult;
            }
            var isCheckUserOrder = orderResult?.Content?.Result ?? default;
            if (!isCheckUserOrder)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(isCheckUserOrder));
                return methodResult;
            }
            var course = await _courseRepository.GetByIdAsync(@class.CourseId);
            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(course));
                return methodResult;
            }

            var unitResult = await _unitResultRepository.Queryable.FirstOrDefaultAsync(x => x.StudentId == request.StudentId && x.CourseId == course.Id && x.Status == EnumResultStatus.Process, cancellationToken);
            if (unitResult == null)
            {
                methodResult.Result = default;
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            var unit = await _unitRepository.Queryable.Include(x => x.LessonResults.Where(x => x.StudentId == request.StudentId)).Include(x => x.UnitLessons).FirstOrDefaultAsync(x => x.Id == unitResult.UnitId, cancellationToken);
            if (unit == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(unit));
                return methodResult;
            }
            methodResult.Result = ((double)unit.LessonResults.Where(x => x.Status == EnumResultStatus.Done).ToList().Count / unit.UnitLessons.ToList().Count, default);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
