// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.UnitQuery
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.Enums.ErrorCodes;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using MediatR;
    using Microsoft.AspNetCore.Http;

    public class GetLessonByUnitQuery : IRequest<MethodResult<LessonModel>>
    {
    }

    public class GetLessonByUnitQueryHandler : IRequestHandler<GetLessonByUnitQuery, MethodResult<LessonModel>>
    {
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly IUnitLessonResultRepository _lessonStudentRepository;
        private readonly AuthContext _authContext;

        public GetLessonByUnitQueryHandler(IMapper mapper,
            IUserService userService,
            IUnitLessonResultRepository lessonStudentRepository,
            AuthContext authContext,
            ILessonRepository lessonRepository
            )
        {
            _userService = userService;
            _authContext = authContext;
        }

        public async Task<MethodResult<LessonModel>> Handle(GetLessonByUnitQuery request, CancellationToken cancellationToken)
        {
            var methodResult = new MethodResult<LessonModel>();

            var user = _authContext.CurrentUserId.ToString();
            var student = await _userService.GetStudentByUserIdAsync(user);
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumCourseErrorCode.NotStudent));
                return methodResult;
            }
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
