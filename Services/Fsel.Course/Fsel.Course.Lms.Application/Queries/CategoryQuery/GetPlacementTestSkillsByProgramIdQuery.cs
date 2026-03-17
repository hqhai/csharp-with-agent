// Copyright (c) Atlantic. All rights reserved.

namespace Fsel.Course.Lms.Application.Queries.CategoryQuery
{
    using Fsel.Common.ActionResults;
    using Fsel.Common.Enums.ErrorCodes;
    using Fsel.Core.Base;
    using Fsel.Course.Domain.IRepositories;
    using Fsel.Course.Domain.Models.EntityModels.SkillModels;
    using Fsel.Course.Lms.Application.Queries.CourseChangeQuery;
    using Fsel.Course.Lms.Application.Services.ApplicationServices.CacheServices;
    using Fsel.Course.Lms.Application.Services.UserServices;
    using Fsel.Shared.Enums.ErrorCodes;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public class GetPlacementTestSkillsByProgramIdQuery : IRequest<MethodResult<IList<SkillModel>>>
    {
    }

    public class GetPlacementTestSkillsByProgramIdQueryHandler : IRequestHandler<GetPlacementTestSkillsByProgramIdQuery, MethodResult<IList<SkillModel>>>
    {
        private readonly ISkillCachingService _skillCachingService;
        private readonly IUserService _userService;
        private readonly AuthContext _authContext;
        private readonly ITestGroupResultRepository _testGroupResultRepository;

        public GetPlacementTestSkillsByProgramIdQueryHandler(ISkillCachingService skillCachingService,
            IUserService userService,
            AuthContext authContext,
            ITestGroupResultRepository testGroupResultRepository
            )
        {
            _skillCachingService = skillCachingService;
            _userService = userService;
            _authContext = authContext;
            _testGroupResultRepository = testGroupResultRepository;
        }

        public async Task<MethodResult<IList<SkillModel>>> Handle(GetPlacementTestSkillsByProgramIdQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            var methodResult = new MethodResult<IList<SkillModel>>();

            var studentResult = await _userService.GetStudentByUserIdAsync(_authContext.CurrentUserId);
            if (!studentResult.IsSuccessStatusCode)
            {
                methodResult.AddErrorBadRequest(nameof(EnumServicesErrorCode.CallUserServiceError), nameof(studentResult));
                return methodResult;
            }

            var student = studentResult.Content?.Result;
            if (student == null)
            {
                methodResult.AddErrorBadRequest(nameof(EnumSystemErrorCode.DataNotExist), nameof(student));
                return methodResult;
            }

            var ptResult = await _testGroupResultRepository.ReadQueryable
                .Where(x => x.StudentId == student.Id && x.TestType == Domain.Enums.EnumTestType.PlacementTest)
                .OrderByDescending(x => x.CreatedDate)
                .FirstOrDefaultAsync(cancellationToken);

            var programId = ptResult?.ProgramIdOfPt ?? default;
            methodResult.Result = await _skillCachingService.GetSkillsPtByProgramIdAsync(programId, cancellationToken);
            return methodResult;
        }
    }
}
