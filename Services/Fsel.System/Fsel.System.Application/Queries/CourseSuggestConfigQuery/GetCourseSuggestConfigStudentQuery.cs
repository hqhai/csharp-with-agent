// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.System.Application.Queries.CourseSuggestConfigQuery
{
    using AutoMapper;
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Shared.Enums.ErrorCodes;
    using Fsel.System.Application.Services.UserServices;
    using Fsel.System.Domain.IRepositories;
    using Fsel.System.Domain.Models.EntityModels;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;

    public class GetCourseSuggestConfigStudentQuery : IRequest<MethodResult<IList<CourseSuggestConfigStudentModel>>>
    {
    }

    public class GetCourseSuggestConfigStudentQueryHandler : IRequestHandler<GetCourseSuggestConfigStudentQuery, MethodResult<IList<CourseSuggestConfigStudentModel>>>
    {
        private readonly ICourseSuggestConfigRepository _courseSuggestConfigRepository;
        private readonly AuthContext _authContext;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public GetCourseSuggestConfigStudentQueryHandler(ICourseSuggestConfigRepository courseSuggestConfigRepository,
                                                         AuthContext authContext,
                                                         IUserService userService,
                                                         IMapper mapper)
        {
            _courseSuggestConfigRepository = courseSuggestConfigRepository;
            _authContext = authContext;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<MethodResult<IList<CourseSuggestConfigStudentModel>>> Handle(GetCourseSuggestConfigStudentQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            MethodResult<IList<CourseSuggestConfigStudentModel>> methodResult = new MethodResult<IList<CourseSuggestConfigStudentModel>>();

            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return methodResult;
            }

            var student = studentResult?.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }

            if (!student.BaseCourseLevel.HasValue)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student.BaseCourseLevel));
                return methodResult;
            }

            int age = Shared.Helpers.DateTimeHelper.GetYearOld(student.User?.Birthday);

            var courseSuggestConfigs = await _courseSuggestConfigRepository.Queryable
                                                                           .Where(x => x.PlacementTestLevel == student.BaseCourseLevel && x.FromAge <= age && x.ToAge >= age)
                                                                           .ToListAsync(cancellationToken);
            if (courseSuggestConfigs == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(courseSuggestConfigs));
                return methodResult;
            }

            methodResult.Result = _mapper.Map<IList<CourseSuggestConfigStudentModel>>(courseSuggestConfigs);
            methodResult.StatusCode = StatusCodes.Status200OK;
            return methodResult;
        }
    }
}
