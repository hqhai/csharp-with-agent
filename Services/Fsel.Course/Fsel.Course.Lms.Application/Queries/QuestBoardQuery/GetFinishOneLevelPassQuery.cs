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
    using Fsel.Shared.Enums;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.Shared.Models.ShareModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetFinishOneLevelPassQuery : IRequest<MethodResult<QuestBoardCategoryModel>>
    {
        public Guid StudentId { get; set; }
        public Guid CurrentUserId { get; set; }
    }

    public class GetFinishOneLevelPassQueryHandler : IRequestHandler<GetFinishOneLevelPassQuery, MethodResult<QuestBoardCategoryModel>>
    {
        private readonly ITrainingService _trainingService;
        private readonly IOrderService _orderService;
        private readonly ICourseRepository _courseRepository;

        public GetFinishOneLevelPassQueryHandler(ITrainingService trainingService
            , IOrderService orderService
            , ICourseRepository courseRepository)
        {
            _trainingService = trainingService;
            _orderService = orderService;
            _courseRepository = courseRepository;
        }

        public async Task<MethodResult<QuestBoardCategoryModel>> Handle(GetFinishOneLevelPassQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<QuestBoardCategoryModel> methodResult = new MethodResult<QuestBoardCategoryModel>();
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

            var orderResult = await _orderService.GetStatusAsync(new GetStatusByUserCommandModel { CourseId = @class.CourseId, UserId = request.CurrentUserId });
            if (!orderResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallOrderServiceError));
                return methodResult;
            }
            var status = orderResult?.Content?.Result ?? default;
            if (status != EnumOrderStatus.Payment)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(status));
                return methodResult;
            }
            var course = await _courseRepository.Queryable.Include(x => x.UnitResults.Where(x => x.StudentId == request.StudentId))
                                                        .Include(x => x.CourseResults.Where(x => x.StudentId == request.StudentId && x.CourseId == @class.CourseId))
                                                        .Include(x => x.CourseUnitMockTests)
                                                        .FirstOrDefaultAsync(x => x.Id == @class.CourseId, cancellationToken);
            if (course == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(course));
                return methodResult;
            }
            QuestBoardCategoryModel questBoardCategoryModel = new QuestBoardCategoryModel();
            questBoardCategoryModel.Percent = (double)course.UnitResults.Where(x => x.Status == EnumResultStatus.Done).ToList().Count / course.CourseUnitMockTests.Where(x => x.UnitId != null).ToList().Count;
            questBoardCategoryModel.ObjectId = course.CourseResults.FirstOrDefault(x => x.StudentId == request.StudentId && x.CourseId == @class.CourseId)?.Id;
            methodResult.Result = questBoardCategoryModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
