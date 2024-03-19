// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.ErrorReportQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.System.Application.Services.CourseServices;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using global::System;
    using global::System.Threading.Tasks;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetErrorReportQuery : IRequest<MethodResult<ErrorReportModel>>
    {
        public Guid Id { get; set; }
    }

    public class GetErrorReportQueryHandler : IRequestHandler<GetErrorReportQuery, MethodResult<ErrorReportModel>>
    {
        private readonly IMapper _mapper;
        private readonly IErrorReportRepository _errorReportRepository;
        private readonly ICourseService _courseService;

        public GetErrorReportQueryHandler(IMapper mapper, IErrorReportRepository errorReportRepository, ICourseService courseService)
        {
            _mapper = mapper;
            _errorReportRepository = errorReportRepository;
            _courseService = courseService;
        }

        public async Task<MethodResult<ErrorReportModel>> Handle(GetErrorReportQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<ErrorReportModel>();

            var errorReport = await _errorReportRepository.GetByIdAsync(request.Id);

            if (errorReport == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(errorReport));
                return methodResult;
            }
            var errorReportModel = _mapper.Map<ErrorReportModel>(errorReport);
            List<Guid> unitIds = new List<Guid>() { errorReportModel.UnitId ?? default };
            List<Guid> courseIds = new List<Guid>() { errorReportModel.CourseId ?? default };
            List<Guid> lessonIds = new List<Guid>() { errorReportModel.LessonId ?? default };

            var courseResults = await _courseService.GetListCourseByIds(courseIds);
            if (!courseResults.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallCourseServiceError), nameof(courseResults));
                return methodResult;
            }
            var courses = courseResults.Content?.Result;

            var unitResults = await _courseService.GetListUnitByIds(unitIds);
            if (!unitResults.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallCourseServiceError), nameof(unitResults));
                return methodResult;
            }
            var units = unitResults.Content?.Result;

            var lessonResults = await _courseService.GetListLessonByIds(lessonIds);
            if (!lessonResults.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallCourseServiceError), nameof(lessonResults));
                return methodResult;
            }
            var lessons = lessonResults.Content?.Result;
            errorReportModel.CourseType = courses?.FirstOrDefault(x => x.Id == errorReportModel.CourseId)?.CourseType ?? default;
            errorReportModel.CourseLevel = courses?.FirstOrDefault(x => x.Id == errorReportModel.CourseId)?.CourseLevel ?? default;
            errorReportModel.CourseName = courses?.FirstOrDefault(x => x.Id == errorReportModel.CourseId)?.Name;
            errorReportModel.UnitName = units?.FirstOrDefault(x => x.Id == errorReportModel.UnitId)?.Name;
            errorReportModel.LessonName = lessons?.FirstOrDefault(x => x.Id == errorReportModel.LessonId)?.Name;

            methodResult.Result = errorReportModel;
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
