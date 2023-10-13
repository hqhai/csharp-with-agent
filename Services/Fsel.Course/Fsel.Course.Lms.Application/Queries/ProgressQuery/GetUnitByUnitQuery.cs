// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.ProgressQuery
{
    using System.Threading;
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

    public class GetUnitByUnitQuery : IRequest<MethodResult<IList<UnitModel>>>
    {
        public Guid CourseId { get; set; }
        public EnumLearnProcessType Type { get; set; }
    }

    public class GetUnitByUnitVideoQueryHandler : IRequestHandler<GetUnitByUnitQuery, MethodResult<IList<UnitModel>>>
    {
        private readonly AuthContext _authContext;
        private readonly ICourseRepository _courseRepository;
        private readonly IUnitRepository _unitRepository;
        private readonly IUserService _userService;

        public GetUnitByUnitVideoQueryHandler(AuthContext authContext
            , ICourseRepository courseRepository
            , IUnitRepository unitRepository
            , IUserService userService)
        {
            _authContext = authContext;
            _courseRepository = courseRepository;
            _unitRepository = unitRepository;
            _userService = userService;
        }

        public async Task<MethodResult<IList<UnitModel>>> Handle(GetUnitByUnitQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<UnitModel>> methodResult = new MethodResult<IList<UnitModel>>();
            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(studentResult));
                return methodResult;
            }
            var studentId = studentResult?.Content?.Result?.Id;
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
            methodResult.Result = await _unitRepository.GetListAsync(studentId, request.CourseId, request.Type);
            return methodResult;
        }
    }
}
