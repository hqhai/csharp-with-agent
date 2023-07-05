// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Commands.CourseResultCmd
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Enums;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class StartCourseResultCommand : IRequest<MethodResult<CourseResultModel>>
    {
        public Guid CourseResultId { get; set; }
    }

    public class StartCourseResultCommandHandler : IRequestHandler<StartCourseResultCommand, MethodResult<CourseResultModel>>
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly AuthContext _authContext;
        private readonly ICourseResultRepository _courseResultRepository;

        public StartCourseResultCommandHandler(IUserService userService
            , IMapper mapper
            , AuthContext authContext
            , ICourseResultRepository courseResultRepository)
        {
            _userService = userService;
            _mapper = mapper;
            _authContext = authContext;
            _courseResultRepository = courseResultRepository;
        }

        public async Task<MethodResult<CourseResultModel>> Handle(StartCourseResultCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<CourseResultModel> methodResult = new MethodResult<CourseResultModel>();

            #region Validation

            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError));
                return methodResult;
            }
            var studentId = studentResult?.Content?.Result?.Id;
            var courseResult = await _courseResultRepository.Queryable.FirstOrDefaultAsync(x => x.Id == request.CourseResultId && x.StudentId == studentId, cancellationToken);
            if (courseResult == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseResultErrorCode.CourseResultsNotExist));
                return methodResult;
            }

            #endregion Validation

            if (courseResult.Status == EnumCourseStatus.New)
            {
                courseResult.Status = EnumCourseStatus.Active;
                _courseResultRepository.Update(courseResult);
                await _courseResultRepository.UnitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            methodResult.StatusCode = StatusCodes.Status200OK;
            methodResult.Result = _mapper.Map<CourseResultModel>(courseResult);
            return methodResult;
        }
    }
}
