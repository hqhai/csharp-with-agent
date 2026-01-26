// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.QuestBoardQuery
{
    using System;
    using System.Threading.Tasks;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Infrastructure.Repositories;
    using Fsel.Course.Lms.Application.Services.OrderServices;
    using Fsel.Course.Lms.Application.Services.OrderServices.Model;
    using Fsel.Course.Lms.Application.Services.TrainingServices;
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetFinishOneUnitQuery : IRequest<MethodResult<QuestBoardCategoryModel>>
    {
        public Guid StudentId { get; set; }
        public Guid CurrentUserId { get; set; }
    }

    public class GetFinishOneUnitQueryHandler : IRequestHandler<GetFinishOneUnitQuery, MethodResult<QuestBoardCategoryModel>>
    {
        private readonly ITrainingService _trainingService;
        private readonly IOrderService _orderService;
        private readonly IUnitRepository _unitRepository;
        private readonly IUnitResultRepository _unitResultRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly ICourseResultRepository _courseResultRepository;

        public GetFinishOneUnitQueryHandler(ITrainingService trainingService
            , IOrderService orderService
            , IUnitRepository unitRepository
            , IUnitResultRepository unitResultRepository
            , ICourseResultRepository courseResultRepository
            , ICourseRepository courseRepository)
        {
            _trainingService = trainingService;
            _orderService = orderService;
            _unitRepository = unitRepository;
            _unitResultRepository = unitResultRepository;
            _courseResultRepository = courseResultRepository;
            _courseRepository = courseRepository;
        }

        public async Task<MethodResult<QuestBoardCategoryModel>> Handle(GetFinishOneUnitQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<QuestBoardCategoryModel> methodResult = new MethodResult<QuestBoardCategoryModel>();
            QuestBoardCategoryModel questBoardCategoryModel = new QuestBoardCategoryModel();

            var classResult = await _trainingService.GetClassToStudentIdAsync(request.StudentId);
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

            var orderResult = await _orderService.GetStatusAsync(new GetStatusByUserCommandModel { UserId = request.CurrentUserId });
            if (!orderResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallOrderServiceError));
                return methodResult;
            }
            var status = orderResult?.Content?.Result;
            if (!status.HasValue || status.Value != EnumOrderStatus.Payment)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(status));
                return methodResult;
            }
            var course = await _courseRepository.GetByIdAsync(@class.CourseId);
            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(course));
                return methodResult;
            }

            var courseResult = await _courseResultRepository.Queryable.FirstOrDefaultAsync(x => x.StudentId == request.StudentId && x.CourseId == course.Id && x.WorkingStatus == EnumWorkingStatus.Active, cancellationToken);
            if (courseResult == null)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            var unitResult = await _unitResultRepository.Queryable.FirstOrDefaultAsync(x => x.CourseResultId == courseResult.Id && x.Status == EnumResultStatus.Process, cancellationToken);
            if (unitResult == null)
            {
                methodResult.StatusCode = StatusCodes.Status200OK;
                return methodResult;
            }
            var unit = await _unitRepository.Queryable.Include(x => x.LessonResults.Where(x => x.StudentId == request.StudentId)).Include(x => x.UnitLessons).FirstOrDefaultAsync(x => x.Id == unitResult.UnitId, cancellationToken);
            if (unit == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(unit));
                return methodResult;
            }
            questBoardCategoryModel.Percent = (double)unit.LessonResults.Where(x => x.Status == EnumResultStatus.Done).ToList().Count / unit.UnitLessons.ToList().Count;
            questBoardCategoryModel.ObjectId = unitResult.Id;
            methodResult.Result = questBoardCategoryModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
